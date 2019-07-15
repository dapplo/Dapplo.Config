//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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

using Dapplo.Config.Interfaces;
using Dapplo.Config.Ini.Implementation;
using Dapplo.Log;
using Dapplo.Config.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Config.Intercepting;
using Timer = System.Timers.Timer;

namespace Dapplo.Config.Ini
{
    /// <summary>
    /// This contains all the ini sections in one ini file
    /// </summary>
    public class IniFileContainer
    {
        private static readonly LogSource Log = new LogSource();
        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        /// <summary>
        /// All the ini sections for this file
        /// </summary>
        private readonly IDictionary<string, IIniSection> _iniSections = new Dictionary<string, IIniSection>(AbcComparer.Instance);
        private readonly IniFileConfig _iniFileConfig;
        private readonly Timer _saveTimer;
        //private readonly AsyncLock _asyncLock = new AsyncLock();
        private FileSystemWatcher _configFileWatcher;
        private IDictionary<string, IDictionary<string, string>> _constants;
        private IDictionary<string, IDictionary<string, string>> _defaults;
        private IDictionary<string, IDictionary<string, string>> _ini = new SortedDictionary<string, IDictionary<string, string>>();

        /// <summary>
        /// The constructor for an IniFileContainer
        /// </summary>
        /// <param name="iniFileConfig">IniFileConfig</param>
        /// <param name="iniSections">IEnumerable of IIniSection</param>
        public IniFileContainer(IniFileConfig iniFileConfig, IEnumerable<IIniSection> iniSections)
        {
            _iniFileConfig = iniFileConfig;

            foreach (var iniSection in iniSections)
            {
                _iniSections[iniSection.GetSectionName()] = iniSection;
            }

            // Look for the ini file, this is only done 1 time.
            IniLocation = CreateFileLocation(false, string.Empty, iniFileConfig.FixedDirectory);

            // Configure the auto save
            if (iniFileConfig.AutoSaveInterval > 0)
            {
                _saveTimer = CreateAutosaveTimer(iniFileConfig.AutoSaveInterval);
            }
            
            Log.Debug().WriteLine("Added IniConfig {0}.{1}", iniFileConfig.ApplicationName, _iniFileConfig.FileName);
            if (iniFileConfig.SaveOnExit)
            {
                // Make sure the configuration is save when the domain is exited
                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Task.Run(async () =>
                {
                    // But only if there was reading from a file
                    if (HasPendingChanges())
                    {
                        await WriteAsync();
                    }
                }).Wait();
            }
        }

        /// <summary>
        ///     Indexer for Ini sections
        /// </summary>
        /// <param name="iniSectionName">string</param>
        /// <returns>IIniSection</returns>
        public IIniSection this[string iniSectionName] => _iniSections[iniSectionName];

        /// <summary>
        /// Check if this IniFileContainer has changes which were not written yet
        /// </summary>
        /// <returns>bool</returns>
        public bool HasPendingChanges()
        {
            foreach (var iniSection in _iniSections.Values)
            {
                if (iniSection.HasChanges())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create a time for the auto-save functionality
        /// </summary>
        /// <param name="autoSaveInterval">uint with the interval</param>
        /// <returns>Timer</returns>
        private Timer CreateAutosaveTimer(uint autoSaveInterval)
        {
            var autosaveTimer = new Timer
            {
                Interval = autoSaveInterval,
                Enabled = true,
                AutoReset = true
            };
            autosaveTimer.Elapsed += async (sender, eventArgs) =>
            {
                // If we didn't read from a file we can stop the "timer tick"

                if (!HasPendingChanges())
                {
                    return;
                }
                try
                {
                    await WriteAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Warn().WriteLine(ex.Message);
                }
            };
            return autosaveTimer;
        }

        /// <summary>
        /// The location for this ini file
        /// </summary>
        public string IniLocation { get; }

        /// <summary>
        ///     Helper to create the location of a file
        /// </summary>
        /// <param name="checkStartupDirectory"></param>
        /// <param name="postfix"></param>
        /// <param name="specifiedDirectory"></param>
        /// <returns>File location</returns>
        private string CreateFileLocation(bool checkStartupDirectory, string postfix = "", string specifiedDirectory = null)
        {
            string file = null;
            if (specifiedDirectory != null)
            {
                file = Path.Combine(specifiedDirectory, $"{_iniFileConfig.FileName}{postfix}.{_iniFileConfig.IniExtension}");
            }
            else
            {
                if (checkStartupDirectory)
                {
                    var startPath = FileLocations.StartupDirectory;
                    if (startPath != null)
                    {
                        file = Path.Combine(startPath, $"{_iniFileConfig.FileName}{postfix}.{_iniFileConfig.IniExtension}");
                    }
                }
                if (file is null || !File.Exists(file))
                {
                    var appDataDirectory = FileLocations.RoamingAppDataDirectory(_iniFileConfig.ApplicationName);
                    file = Path.Combine(appDataDirectory, $"{_iniFileConfig.FileName}{postfix}.{_iniFileConfig.IniExtension}");
                }
            }
            Log.Verbose().WriteLine("File location: {0}", file);
            return file;
        }


        #region Read

        /// <summary>
        ///     Initialize the IniConfig by reading all the properties from the stream
        ///     If this is called directly after construction, no files will be read which is useful for testing!
        /// </summary>
        public async Task ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            // This is for testing, clear all defaults & constants as the 
            _defaults = null;
            _constants = null;
            _ini = await IniFile.ReadAsync(stream, _iniFileConfig.FileEncoding, cancellationToken).ConfigureAwait(false);

            // Reset the current sections
            FillSections();
        }


        #region Fill

        /// <summary>
        ///     Helper method to fill the values of one section
        /// </summary>
        /// <param name="iniSection"></param>
        private void FillSection(IIniSection iniSection)
        {
            if (_saveTimer != null)
            {
                _saveTimer.Enabled = false;
            }

            // Make sure there is no write protection
            iniSection.RemoveWriteProtection();

            // Defaults:
            if (_defaults != null)
            {
                FillSection(_defaults, iniSection);
            }
            // Ini:
            if (_ini != null)
            {
                FillSection(_ini, iniSection);
            }
            // Constants:
            if (_constants != null)
            {
                iniSection.StartWriteProtecting();
                FillSection(_constants, iniSection);
                iniSection.StopWriteProtecting();
            }

            // After load
            var configProxy = iniSection as ConfigProxy;
            var implementation = configProxy.Target as IIniSectionInternal;
            implementation?.OnAfterLoad?.Invoke(iniSection);

            iniSection.ResetHasChanges();

            if (_saveTimer != null)
            {
                _saveTimer.Enabled = true;
            }
        }

        /// <summary>
        ///     Put the values from the iniProperties to the proxied object
        /// </summary>
        /// <param name="iniSections"></param>
        /// <param name="iniSection"></param>
        private void FillSection(IDictionary<string, IDictionary<string, string>> iniSections, IIniSection iniSection)
        {
            var sectionName = iniSection.GetSectionName();
            // Might be null
            iniSections.TryGetValue(sectionName, out var iniProperties);

            var iniValues = from iniValue in iniSection.GetIniValues().Values.ToList()
                            where iniValue.Behavior.Read
                            select iniValue;

            foreach (var iniValue in iniValues)
            {
                ITypeDescriptorContext context = null;
                try
                {
                    var propertyDescription = TypeDescriptor.GetProperties(iniSection.GetType()).Find(iniValue.PropertyName, true);
                    context = new TypeDescriptorContext(iniSection, propertyDescription);
                }
                catch (Exception ex)
                {
                    Log.Warn().WriteLine(ex.Message);
                }

                // Test if there is a separate section for this inivalue, this is used for Dictionaries
                if (iniSections.TryGetValue($"{sectionName}-{iniValue.IniPropertyName}", out var value))
                {
                    try
                    {
                        iniValue.Value = iniValue.ValueType.ConvertOrCastValueToType(value, iniValue.Converter, context);
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Log.Warn().WriteLine(ex.Message);
                    }
                }
                // Skip if the iniProperties doesn't have anything
                if (iniProperties is null || iniProperties.Count == 0)
                {
                    continue;
                }

                // Skip values that don't have a property
                if (!iniProperties.TryGetValue(iniValue.IniPropertyName, out var stringValue))
                {
                    continue;
                }

                // convert
                try
                {
                    iniValue.Value = iniValue.ValueType.ConvertOrCastValueToType(stringValue, iniValue.Converter, context);
                }
                catch (Exception ex)
                {
                    Log.Warn().WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        ///     Internal method, use the supplied ini-sections and properties to fill the sectoins
        /// </summary>
        private void FillSections()
        {
            foreach (var iniSection in _iniSections.Values)
            {
                FillSection(iniSection);
            }
        }

        #endregion

        /// <summary>
        ///     This is reloading all the .ini files, and will refill the sections.
        ///     If reset = true, ALL setting are lost
        ///     Otherwise only the properties in the files will overwrite your settings.
        ///     Usually this should not directly be called, unless you know that the file was changed by an external process.
        /// </summary>
        /// <param name="reset">true: ALL setting are lost</param>
        /// <param name="cancellationToken">CancellationToken</param>
        public async Task ReloadAsync(bool reset = true, CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                await ReloadInternalAsync(reset, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        ///     This is reloading all the .ini files, and will refill the sections.
        ///     If reset = true, ALL setting are lost
        ///     Otherwise only the properties in the files will overwrite your settings.
        ///     Usually this should not directly be called, unless you know that the file was changed by an external process.
        /// </summary>
        /// <param name="reset">true: ALL setting are lost</param>
        /// <param name="cancellationToken">CancellationToken</param>
        private async Task ReloadInternalAsync(bool reset = true, CancellationToken cancellationToken = default)
        {
            if (reset)
            {
                ResetInternal();
            }

            _defaults = await IniFile.ReadAsync(CreateFileLocation(true, _iniFileConfig.DefaultsPostfix, _iniFileConfig.FixedDirectory), _iniFileConfig.FileEncoding, cancellationToken).ConfigureAwait(false);
            _constants = await IniFile.ReadAsync(CreateFileLocation(true, _iniFileConfig.ContantsPostfix, _iniFileConfig.FixedDirectory), _iniFileConfig.FileEncoding, cancellationToken).ConfigureAwait(false);
            var newIni = await IniFile.ReadAsync(IniLocation, _iniFileConfig.FileEncoding, cancellationToken).ConfigureAwait(false);

            // As we readed the file, make sure we enable the event raising (if the file watcher is wanted)
            EnableFileWatcher(true);
            if (newIni != null)
            {
                _ini = newIni;
            }

            // Reset the sections that have already been registered
            FillSections();
            Log.Verbose().WriteLine("Finished reading and filling sections.");
        }

        #endregion

        #region Reset

        /// <summary>
        ///     Reset all the values, in all the registered ini sections, to their defaults
        /// </summary>
        public async Task ResetAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                ResetInternal();
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        ///     Reset all the values, in all the registered ini sections, to their defaults
        ///     Important, this only works for types that extend IDefaultValue
        /// </summary>
        internal void ResetInternal()
        {
            foreach (var iniSection in _iniSections.Values)
            {
                if (iniSection is IDefaultValue defaultValueInterface)
                {
                    foreach (var propertyName in iniSection.PropertyNames())
                    {
                        // TODO: Do we need to skip read/write protected values here?
                        defaultValueInterface.RestoreToDefault(propertyName);
                    }
                    var configProxy = iniSection as ConfigProxy;
                    var implementation = configProxy.Target as IIniSectionInternal;
                    implementation?.OnAfterLoad?.Invoke(iniSection);
                }
            }
        }

        #endregion

        #region Write

        /// <summary>
        ///     Write the ini file
        /// </summary>
        public async Task WriteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                var path = Path.GetDirectoryName(IniLocation);

                // Create the directory to write to, if it doesn't exist yet
                if (path != null && !Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // disable the File-Watcher so we don't get events from ourselves
                EnableFileWatcher(false);

                Log.Debug().WriteLine("Saving to {0}", IniLocation);
                // Create the file as a stream
                using (var stream = new FileStream(IniLocation, FileMode.Create, FileAccess.Write))
                {
                    // Write the registered ini sections to the stream
                    await WriteToStreamInternalAsync(stream, cancellationToken).ConfigureAwait(false);
                }

                // Enable the File-Watcher so we get events again
                EnableFileWatcher(true);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        ///     Write all the IIniSections to the stream, this is also used for testing
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task</returns>
        public async Task WriteToStreamAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
                await WriteToStreamInternalAsync(stream, cancellationToken);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        ///     Store the ini to the supplied stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>Task to await</returns>
        private async Task WriteToStreamInternalAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            var iniSectionsComments = new SortedDictionary<string, IDictionary<string, string>>();

            // Loop over the "registered" sections
            foreach (var iniSection in _iniSections.Values)
            {
                var configProxy = iniSection as ConfigProxy;
                var implementation = configProxy.Target as IIniSectionInternal;
                implementation?.OnBeforeSave?.Invoke(iniSection);

                try
                {
                    CreateSaveValues(iniSection, iniSectionsComments);
                }
                finally
                {
                    implementation?.OnAfterSave?.Invoke(iniSection);
                }
            }
            await IniFile.WriteAsync(stream, _iniFileConfig.FileEncoding, _ini, iniSectionsComments, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///     Helper method to create ini section values for writing.
        ///     The actual values are stored in the _ini
        /// </summary>
        /// <param name="iniSection">Section to write</param>
        /// <param name="iniSectionsComments">Comments</param>
        private void CreateSaveValues(IIniSection iniSection, IDictionary<string, IDictionary<string, string>> iniSectionsComments)
        {
            // This flag tells us if the header for the section is already written
            var isSectionCreated = false;
            var sectionName = iniSection.GetSectionName();

            var sectionProperties = new SortedDictionary<string, string>();
            var sectionComments = new SortedDictionary<string, string>();
            // Loop over the ini values, this automatically skips all NonSerialized properties
            foreach (var iniValue in iniSection.GetIniValues().Values.ToList())
            {
                // Check if we need to write the value, this is not needed when it has the default or if write is disabled
                if (!iniValue.IsWriteNeeded)
                {
                    // Remove the value, if it's still in the cache
                    if (_ini.ContainsKey(sectionName) && _ini[sectionName].ContainsKey(iniValue.PropertyName))
                    {
                        _ini[sectionName].Remove(iniValue.PropertyName);
                    }
                    continue;
                }

                // Before we are going to write, we need to check if the section header "[Sectionname]" is already written.
                // If not, do so now before writing the properties of the section itself
                if (!isSectionCreated)
                {
                    if (_ini.ContainsKey(sectionName))
                    {
                        _ini.Remove(sectionName);
                    }
                    _ini.Add(sectionName, sectionProperties);
                    iniSectionsComments.Add(sectionName, sectionComments);

                    var description = iniSection.GetSectionDescription();
                    if (!string.IsNullOrEmpty(description))
                    {
                        sectionComments.Add(sectionName, description);
                    }
                    // Mark section as created!
                    isSectionCreated = true;
                }

                // Check if the property has a description, if so write it in the ini comment before the property
                if (!string.IsNullOrEmpty(iniValue.Description))
                {
                    sectionComments.Add(iniValue.IniPropertyName, iniValue.Description);
                }

                ITypeDescriptorContext context = null;
                try
                {
                    var propertyDescription = TypeDescriptor.GetProperties(iniSection.GetType()).Find(iniValue.PropertyName, true);
                    context = new TypeDescriptorContext(iniSection, propertyDescription);
                }
                catch (Exception ex)
                {
                    Log.Warn().WriteLine(ex.Message);
                }

                // Get specified converter
                var converter = iniValue.Converter;

                // Special case, for idictionary derrivated types
                if (iniValue.ValueType.IsGenericType && iniValue.ValueType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var subSection = TypeExtensions.ConvertOrCastValueToType<IDictionary<string, string>>(iniValue.Value, converter, context, false);
                    if (subSection != null)
                    {
                        try
                        {
                            // Use this to build a separate "section" which is called "[section-propertyname]"
                            string dictionaryIdentifier = $"{sectionName}-{iniValue.IniPropertyName}";
                            if (_ini.ContainsKey(dictionaryIdentifier))
                            {
                                _ini.Remove(dictionaryIdentifier);
                            }
                            _ini.Add(dictionaryIdentifier, subSection);
                            if (!string.IsNullOrWhiteSpace(iniValue.Description))
                            {
                                var dictionaryComments = new SortedDictionary<string, string>
                                {
                                    {dictionaryIdentifier, iniValue.Description}
                                };
                                iniSectionsComments.Add(dictionaryIdentifier, dictionaryComments);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn().WriteLine(ex.Message);
                            //WriteErrorHandler(iniSection, iniValue, ex);
                        }
                        continue;
                    }
                }

                try
                {
                    // Convert the value to a string
                    var writingValue = TypeExtensions.ConvertOrCastValueToType<string>(iniValue.Value, converter, context, false);
                    // And write the value with the IniPropertyName (which does NOT have to be the property name) to the file
                    sectionProperties.Add(iniValue.IniPropertyName, writingValue);
                }
                catch (Exception ex)
                {
                    Log.Warn().WriteLine(ex.Message);
                    //WriteErrorHandler(iniSection, iniValue, ex);
                }
            }
        }

        #endregion


        /// <summary>
        ///     Create a FileSystemWatcher to detect changes
        /// </summary>
        /// <param name="enable">true to enable the watcher</param>
        private void EnableFileWatcher(bool enable)
        {
            if (!_iniFileConfig.WatchFileChanges)
            {
                return;
            }

            // If it is already created, just change the enable
            if (_configFileWatcher != null)
            {
                _configFileWatcher.EnableRaisingEvents = enable;
                return;
            }

            if (!enable)
            {
                // if it is not created, and enable = false, do nothing
                return;
            }

            // If the ini-location directory is not yet created, we can't watch as this would cause an exception
            var watchPath = Path.GetDirectoryName(IniLocation);
            if (watchPath is null || !Directory.Exists(watchPath))
            {
                return;
            }

            // Configure file change watching
            _configFileWatcher = new FileSystemWatcher
            {
                Path = watchPath,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(IniLocation),
                EnableRaisingEvents = true
            };

            // add change handling
            _configFileWatcher.Changed += async (sender, eventArgs) =>
            {
                try
                {
                    // Disable events before
                    _configFileWatcher.EnableRaisingEvents = false;
                    await ReloadAsync(false);
                }
                catch (Exception ex)
                {
                    Log.Warn().WriteLine(ex.Message);
                }
                finally
                {
                    // Disable events after
                    _configFileWatcher.EnableRaisingEvents = true;
                }
            };
        }
    }
}
