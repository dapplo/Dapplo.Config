//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Config
// 
//  Dapplo.Config is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Config is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Language.Implementation;
using Dapplo.Log;
using Dapplo.Utils;

#endregion

namespace Dapplo.Config.Language
{
    /// <summary>
    ///     The language loader should be used to fill ILanguage interfaces.
    ///     It is possible to specify the directory locations, in order, where files with certain patterns should be located.
    /// </summary>
    public sealed class LanguageContainer : IDisposable, IEnumerable<ILanguage>
    {
        private readonly LanguageConfig _languageConfig;
        private static readonly LogSource Log = new LogSource();
        private readonly IDictionary<string, IDictionary<string, string>> _allTranslations = new Dictionary<string, IDictionary<string, string>>(AbcComparer.Instance);
        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly Regex _filePattern;
        private readonly IDictionary<string, ILanguage> _languageConfigs = new Dictionary<string, ILanguage>(AbcComparer.Instance);
        private bool _initialReadDone;

        /// <summary>
        ///     Create a LanguageLoader, this is your container for all the ILanguage implementing interfaces.
        ///     You can supply a default language right away.
        /// </summary>
        public LanguageContainer(LanguageConfig languageConfig, IEnumerable<ILanguage> languageInterfaces)
        {
            _languageConfig = languageConfig;
            foreach (var languageInterface in languageInterfaces)
            {
                _languageConfigs[languageInterface.PrefixName()] = languageInterface;
            }
            CurrentLanguage = languageConfig.DefaultLanguage;
            _filePattern = new Regex(languageConfig.FileNamePattern, RegexOptions.Compiled);
            ScanFiles(languageConfig);
        }

        /// <summary>
        ///     All languages that were found in the files during the scan.
        /// </summary>
        public IDictionary<string, string> AvailableLanguages { get; private set; }

        /// <summary>
        ///     Get the IETF of the current language.
        ///     For the name of the language, use the AvailableLanguages with this value as the key.
        /// </summary>
        public string CurrentLanguage { get; private set; }

        /// <summary>
        ///     The files, ordered to the IETF, that were found during the scan
        /// </summary>
        public IDictionary<string, List<string>> Files { get; private set; }

        /// <summary>
        ///     Get the specified ILanguage type
        /// </summary>
        /// <param name="prefix">ILanguage prefix to look for</param>
        /// <returns>ILanguage</returns>
        public ILanguage this[string prefix] => _languageConfigs[prefix];

        /// <summary>
        ///     Change the language, this will only do something if the language actually changed.
        ///     All files are reloaded.
        /// </summary>
        /// <param name="ietf">The iso code for the language to use</param>
        /// <param name="cancellationToken">CancellationToken for the loading</param>
        /// <returns>Task</returns>
        public async Task ChangeLanguageAsync(string ietf, CancellationToken cancellationToken = default)
        {
            if (ietf == CurrentLanguage)
            {
                return;
            }
            Log.Verbose().WriteLine("Changing language to {0}", ietf);
            if (AvailableLanguages.ContainsKey(ietf))
            {
                CurrentLanguage = ietf;
                await ReloadAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Log.Warn().WriteLine("Language {0} was not available.", ietf);
            }
        }

        /// <summary>
        ///     Call this to make sure that all languages have translations.
        ///     This will walk through the files of the supplied language (or the one with the most translations)
        ///     and copy the "missing" files to the others.
        ///     By doing this, all non translated components will be in the mostly used translation.
        ///     TODO: Use system language?
        /// </summary>
        public void CorrectMissingTranslations()
        {
            if (Files is null || Files.Count == 0)
            {
                return;
            }
            var baseIetf = (from ietf in Files.Keys
                select new
                {
                    ietf,
                    Files[ietf].Count
                }).OrderByDescending(x => x.Count).FirstOrDefault()?.ietf;
            if (baseIetf is null)
            {
                return;
            }
            var baseFileList = Files[baseIetf].ToList();
            foreach (var ietf in Files.Keys.ToList())
            {
                if (ietf == baseIetf)
                {
                    continue;
                }
                var comparingFiles = Files[ietf].Select(Path.GetFileNameWithoutExtension).ToList();
                // Even if the count matches, there could be different files
                foreach (var file in baseFileList)
                {
                    var possibleTargetFile = Path.GetFileNameWithoutExtension(file.Replace(baseIetf, ietf));

                    if (comparingFiles.Contains(possibleTargetFile))
                    {
                        continue;
                    }
                    // Add missing translation
                    Log.Verbose().WriteLine("Added missing file {0}", file);
                    Files[ietf].Add(file);
                }
            }
        }

        /// <summary>
        ///     Fill the backing properties of the supplied object.
        ///     Match the ini-file properties with the name of the property.
        /// </summary>
        /// <param name="language"></param>
        private void FillLanguageConfig(ILanguage language)
        {
            if (!(language is ConfigurationBase<string> configurationBase))
            {
                throw new NotSupportedException("Implementation behind the ILanguage is not a ConfigurationBase");
            }

            var prefix = language.PrefixName();

            if (!_allTranslations.TryGetValue(prefix, out var sectionTranslations))
            {
                // No values, reset all (only available via the PropertyTypes dictionary
                foreach (var key in language.PropertyNames().ToList())
                {
                    language.RestoreToDefault(key);
                }
                return;
            }

            // Use PropertyTypes.Keys to get ALL possible properties.
            foreach (var key in language.PropertyNames().ToList())
            {
                if (sectionTranslations.TryGetValue(key, out var translation))
                {
                    configurationBase.Setter(key, translation);
                    sectionTranslations.Remove(key);
                }
                else
                {
                    language.RestoreToDefault(key);
                }
            }

            // Add all unprocessed values
            foreach (var key in sectionTranslations.Keys.ToList())
            {
                var translation = sectionTranslations[key];
                configurationBase.Setter(key, translation);
            }

            // Generate the language changed event
            // Added for Dapplo.Config/issues/10
            var languageInternal = (ILanguageInternal) language;
            languageInternal?.OnLanguageChanged();
        }

        /// <summary>
        ///     Start the intial load, but if none was made yet
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public async Task LoadIfNeededAsync(CancellationToken cancellationToken = default)
        {
            if (!_initialReadDone)
            {
                await ReloadAsync(cancellationToken);
            }
        }

        /// <summary>
        ///     Read the resources from the specified file
        /// </summary>
        /// <param name="languageFile"></param>
        /// <returns>name - values sorted to module</returns>
        private static IDictionary<string, IDictionary<string, string>> ReadXmlResources(string languageFile)
        {
            var xElement = XDocument.Load(languageFile).Root;
            if (xElement is null)
            {
                return null;
            }
            return (from resourcesElement in xElement.Elements("resources")
                where resourcesElement.Attribute("prefix") != null
                from resourceElement in resourcesElement.Elements("resource")
                group resourceElement by resourcesElement.Attribute("prefix")?.Value ?? "empty"
                into resourceElementGroup
                select resourceElementGroup).ToDictionary(group => group.Key,
                group => (IDictionary<string, string>) group.ToDictionary(x => x?.Attribute("name")?.Value ?? "empty", x => x.Value.Trim()));
        }

        /// <summary>
        ///     This is reloading all the .ini files, and will refill the language objects.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task ReloadAsync(CancellationToken cancellationToken = default)
        {
            _allTranslations.Clear();
            if (Files.ContainsKey(CurrentLanguage))
            {
                await LoadLanguageFiles(cancellationToken);
            }
            _initialReadDone = true;

            // Reset the sections that have already been registered
            foreach (var language in _languageConfigs.Values)
            {
                FillLanguageConfig(language);
            }
        }

        /// <summary>
        /// Helper method to load the language files
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        private async Task LoadLanguageFiles(CancellationToken cancellationToken)
        {
            foreach (var languageFile in Files[CurrentLanguage].ToList())
            {
                IDictionary<string, IDictionary<string, string>> newResources;
                if (languageFile.EndsWith(".ini"))
                {
                    newResources = await IniFile.ReadAsync(languageFile, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
                }
                else if (languageFile.EndsWith(".xml"))
                {
                    newResources = ReadXmlResources(languageFile);
                }
                else
                {
                    throw new NotSupportedException($"Can't read the file format for {languageFile}");
                }
                if (newResources is null)
                {
                    continue;
                }
                foreach (var section in newResources.Keys.ToList())
                {
                    var properties = newResources[section];
                    if (!_allTranslations.TryGetValue(section, out var sectionTranslations))
                    {
                        sectionTranslations = new Dictionary<string, string>(AbcComparer.Instance);
                        _allTranslations.Add(section, sectionTranslations);
                    }
                    foreach (var key in properties.Keys.ToList())
                    {
                        sectionTranslations[key] = properties[key];
                    }
                }
            }
        }


        /// <summary>
        ///     Helper to create the location of a file
        /// </summary>
        private void ScanFiles(LanguageConfig languageConfig)
        {
            var directories = new List<string>();
            if (languageConfig.SpecifiedDirectories != null)
            {
                directories.AddRange(languageConfig.SpecifiedDirectories);
            }
            if (languageConfig.CheckStartupDirectory)
            {
                var startupDirectory = FileLocations.StartupDirectory;
                if (startupDirectory != null)
                {
                    directories.Add(Path.Combine(startupDirectory, "languages"));
                }
            }
            if (languageConfig.CheckAppDataDirectory)
            {
                var appDataDirectory = FileLocations.RoamingAppDataDirectory(languageConfig.ApplicationName);
                if (appDataDirectory != null)
                {
                    directories.Add(Path.Combine(appDataDirectory, "languages"));
                }
            }

            if (Log.IsDebugEnabled())
            {
                Log.Debug().WriteLine("Scanning directories: {0}", string.Join(",", directories));
            }

            Files = FileLocations.Scan(directories, _filePattern)
                .GroupBy(x => x.Item2.Groups["IETF"].Value)
                .ToDictionary(group => group.Key, group => group.Select(x => x.Item1)
                    .ToList());

            if (Log.IsDebugEnabled())
            {
                Log.Debug().WriteLine("Detected language ietfs: {0}", string.Join(",", Files.Keys));
            }

            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .ToLookup(e => e.IetfLanguageTag, StringComparer.OrdinalIgnoreCase).ToDictionary(x => x.Key, x => x.First());

            // TODO: Create custom culture for all not available, see: https://msdn.microsoft.com/en-us/library/ms172469(v=vs.90).aspx
            // TODO: Add Embedded resources, especially for the assemblies to which ILanguage interfaces belong

            AvailableLanguages = (from ietf in Files.Keys
                where allCultures.ContainsKey(ietf)
                select ietf).Distinct().ToDictionary(ietf => ietf, ietf => allCultures[ietf].NativeName);

            if (Log.IsVerboseEnabled())
            {
                Log.Verbose().WriteLine("Languages found: {0}", string.Join(",", AvailableLanguages.Keys));
            }
        }

        #region IDisposable Support

        // To detect redundant Dispose calls
        private bool _disposedValue;

        /// <summary>
        ///     Dispose implementation
        /// </summary>
        public void Dispose()
        {
            if (_disposedValue)
            {
                return;
            }
            _asyncLock?.Dispose();
            _disposedValue = true;
        }

        #endregion
        
        /// <inheritdoc />
        public IEnumerator<ILanguage> GetEnumerator()
        {
            return _languageConfigs.Values.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}