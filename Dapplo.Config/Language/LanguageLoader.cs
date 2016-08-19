//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dapplo.Config.Ini;
using Dapplo.Config.Language.Implementation;
using Dapplo.InterfaceImpl;
using Dapplo.InterfaceImpl.Extensions;
using Dapplo.Log.Facade;
using Dapplo.Utils;

#endregion

namespace Dapplo.Config.Language
{
	/// <summary>
	///     The language loader should be used to fill ILanguage interfaces.
	///     It is possible to specify the directory locations, in order, where files with certain patterns should be located.
	/// </summary>
	public class LanguageLoader : IConfigProvider
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly IDictionary<string, LanguageLoader> LoaderStore = new Dictionary<string, LanguageLoader>(new AbcComparer());
		private readonly IDictionary<string, IDictionary<string, string>> _allTranslations = new Dictionary<string, IDictionary<string, string>>(new AbcComparer());
		private readonly string _applicationName;
		private readonly AsyncLock _asyncLock = new AsyncLock();
		private readonly Regex _filePattern;
		private readonly IDictionary<string, ILanguage> _languageConfigs = new Dictionary<string, ILanguage>(new AbcComparer());
		private readonly IDictionary<Type, ILanguage> _languageTypeConfigs = new Dictionary<Type, ILanguage>();
		private bool _initialReadDone;

		/// <summary>
		///     Define some static constants which could not be assigned directly
		/// </summary>
		static LanguageLoader()
		{
			InterceptorFactory.DefineBaseTypeForInterface(typeof (ILanguage), typeof (Language<>));
			InterceptorFactory.DefineDefaultInterfaces(typeof (ILanguage), new[] {typeof (IDefaultValue), typeof (IHasChanges)});
		}

		/// <summary>
		///     Create a LanguageLoader, this is your container for all the ILanguage implementing interfaces.
		///     You can supply a default language right away.
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="defaultLanguage"></param>
		/// <param name="filePatern">Pattern for the filename, the ietf group needs to be in there!</param>
		/// <param name="checkStartupDirectory"></param>
		/// <param name="checkAppDataDirectory"></param>
		/// <param name="specifiedDirectories"></param>
		public LanguageLoader(string applicationName, string defaultLanguage = "en-US",
			string filePatern = @"language(_(?<module>[a-zA-Z0-9]*))?-(?<IETF>[a-zA-Z]{2}(-[a-zA-Z]+)?-[a-zA-Z]+)\.(ini|xml)", bool checkStartupDirectory = true,
			bool checkAppDataDirectory = true, ICollection<string> specifiedDirectories = null)
		{
			lock (LoaderStore)
			{
				if (LoaderStore.ContainsKey(applicationName))
				{
					throw new InvalidOperationException($"{applicationName} was already created!");
				}
				CurrentLanguage = defaultLanguage;
				_filePattern = new Regex(filePatern, RegexOptions.Compiled);
				_applicationName = applicationName;
				ScanFiles(checkStartupDirectory, checkAppDataDirectory, specifiedDirectories);
				Log.Debug().WriteLine("Adding {0}", applicationName);
				LoaderStore[applicationName] = this;
			}
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
		public ILanguage this[string prefix]
		{
			get
			{
				lock (_languageConfigs)
				{
					return _languageConfigs[prefix];
				}
			}
		} 

		/// <summary>
		///     Change the language, this will only do something if the language actually changed.
		///     All files are reloaded.
		/// </summary>
		/// <param name="ietf">The iso code for the language to use</param>
		/// <param name="cancellationToken">CancellationToken for the loading</param>
		/// <returns>Task</returns>
		public async Task ChangeLanguageAsync(string ietf, CancellationToken cancellationToken = default(CancellationToken))
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
			if (Files == null || Files.Count == 0)
			{
				return;
			}
			var baseIetf = (from ietf in Files.Keys
				select new
				{
					ietf,
					Files[ietf].Count
				}).OrderByDescending(x => x.Count).FirstOrDefault()?.ietf;
			if (baseIetf == null)
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
			var prefix = language.PrefixName();
			IDictionary<string, string> sectionTranslations;
			// ReSharper disable once SuspiciousTypeConversion.Global
			var defaultValueInterface = language as IDefaultValue;
			var interceptor = language as IExtensibleInterceptor;
			if (interceptor == null || defaultValueInterface == null)
			{
				throw new NullReferenceException("Should not happen.");
			}
			if (!_allTranslations.TryGetValue(prefix, out sectionTranslations))
			{
				// No values, reset all (only available via the PropertyTypes dictionary
				foreach (var key in interceptor.PropertyTypes.Keys.ToList())
				{
					defaultValueInterface.RestoreToDefault(key);
				}
				return;
			}

			// Use PropertyTypes.Keys to get ALL possible properties.
			foreach (var key in interceptor.PropertyTypes.Keys.ToList())
			{
				string translation;
				if (sectionTranslations.TryGetValue(key, out translation))
				{
					interceptor.Set(key, translation);
					sectionTranslations.Remove(key);
				}
				else
				{
					defaultValueInterface.RestoreToDefault(key);
				}
			}

			// Add all unprocessed values
			foreach (var key in sectionTranslations.Keys.ToList())
			{
				interceptor.Properties[key] = sectionTranslations[key];
			}

			// Generate the language changed event
			// Added for Dapplo.Config/issues/10
			var languageInternal = language as ILanguageInternal;
			languageInternal?.OnLanguageChanged();
		}

		/// <summary>
		///     Get or register/get Interface to this language loader, this method will return the filled property object
		/// </summary>
		/// <param name="type">ILanguage Type</param>
		/// <returns>object (which is a ILanguage)</returns>
		public object Get(Type type)
		{
			if (!_initialReadDone)
			{
				throw new InvalidOperationException("Please load before retrieving the language");
			}
			ILanguage language;
			// Make sure we lock, to prevent double adding
			lock (_languageTypeConfigs)
			{
				if (_languageTypeConfigs.TryGetValue(type, out language))
				{
					// Already filled type
					return language;
				}
				// TODO: scan the location of the assembly where the type is, if it hasn't been scanned, for language files.
				// TODO: Also scan the embedded resource of the ILanguage containing assembly
				language = (ILanguage)InterceptorFactory.New(type);
				_languageTypeConfigs.Add(type, language);
				_languageConfigs[language.PrefixName()] = language;
			}
			FillLanguageConfig(language);

			return language;
		}


		/// <summary>
		///     Get or register/get Interface to this language loader, this method will return the filled property object
		/// </summary>
		/// <typeparam name="T">ILanguage</typeparam>
		/// <returns>T</returns>
		public T Get<T>() where T : ILanguage
		{
			var type = typeof (T);
			return (T) Get(type);
		}

		/// <summary>
		///     Start the intial load, but if none was made yet
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Task</returns>
		public async Task LoadIfNeededAsync(CancellationToken cancellationToken  = default(CancellationToken))
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
		private IDictionary<string, IDictionary<string, string>> ReadXmlResources(string languageFile)
		{
			var xElement = XDocument.Load(languageFile).Root;
			if (xElement == null)
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
		///     Register a Property Interface to this ini config, this method will return the property object
		/// </summary>
		/// <typeparam name="T">Your property interface, which extends IIniSection</typeparam>
		/// <returns>instance of type T</returns>
		public async Task<T> RegisterAndGetAsync<T>(CancellationToken cancellationToken = default(CancellationToken)) where T : ILanguage
		{
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				await LoadIfNeededAsync(cancellationToken);
				return Get<T>();
			}
		}

		/// <summary>
		///     This is reloading all the .ini files, and will refill the language objects.
		/// </summary>
		/// <param name="cancellationToken">CancellationToken</param>
		public async Task ReloadAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			_allTranslations.Clear();
			if (Files.ContainsKey(CurrentLanguage))
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
					if (newResources == null)
					{
						continue;
					}
					foreach (var section in newResources.Keys.ToList())
					{
						var properties = newResources[section];
						IDictionary<string, string> sectionTranslations;
						if (!_allTranslations.TryGetValue(section, out sectionTranslations))
						{
							sectionTranslations = new Dictionary<string, string>(new AbcComparer());
							_allTranslations.Add(section, sectionTranslations);
						}
						foreach (var key in properties.Keys.ToList())
						{
							sectionTranslations[key] = properties[key];
						}
					}
				}
			}
			_initialReadDone = true;

			IList<ILanguage> languageObjectsToFill;
			lock (_languageTypeConfigs)
			{
				languageObjectsToFill = _languageTypeConfigs.Values.ToList();
			}
			// Reset the sections that have already been registered
			foreach (var language in languageObjectsToFill)
			{
				FillLanguageConfig(language);
			}
		}


		/// <summary>
		///     Helper to create the location of a file
		/// </summary>
		/// <param name="checkStartupDirectory"></param>
		/// <param name="checkAppDataDirectory"></param>
		/// <param name="specifiedDirectories">Specify your own directory</param>
		private void ScanFiles(bool checkStartupDirectory, bool checkAppDataDirectory = true, ICollection<string> specifiedDirectories = null)
		{
			var directories = new List<string>();
			if (specifiedDirectories != null)
			{
				directories.AddRange(specifiedDirectories);
			}
			if (checkStartupDirectory)
			{
				var startupDirectory = FileLocations.StartupDirectory;
				if (startupDirectory != null)
				{
					directories.Add(Path.Combine(startupDirectory, "languages"));
				}
			}
			if (checkAppDataDirectory)
			{
				var appDataDirectory = FileLocations.RoamingAppDataDirectory(_applicationName);
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

		#region Static

		/// <summary>
		///     Static helper to retrieve the LanguageLoader that was created with the supplied parameters
		/// </summary>
		/// <param name="applicationName"></param>
		/// <returns>LanguageLoader</returns>
		public static LanguageLoader Get(string applicationName)
		{
			lock (LoaderStore)
			{
				return LoaderStore[applicationName];
			}
		}

		/// <summary>
		///     Static helper to retrieve the first LanguageLoader that was created
		/// </summary>
		/// <returns>LanguageLoader or null</returns>
		public static LanguageLoader Current => LoaderStore.FirstOrDefault().Value;

		/// <summary>
		///     Delete the Language objects for the specified application, mostly used in tests
		/// </summary>
		/// <param name="applicationName"></param>
		public static void Delete(string applicationName)
		{
			Log.Debug().WriteLine("Removing {0}", applicationName);
			lock (LoaderStore)
			{
				if (!LoaderStore.ContainsKey(applicationName))
				{
					return;
				}
				var loader = LoaderStore[applicationName];
				// Make sure the AsyncLock is disposed
				loader.Dispose();
				LoaderStore.Remove(applicationName);
			}
		}

		#endregion

		#region IDisposable Support

		// To detect redundant Dispose calls
		private bool _disposedValue;

		/// <summary>
		/// Disposes the lock
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposedValue)
			{
				return;
			}
			if (disposing)
			{
				_asyncLock?.Dispose();
			}
			_disposedValue = true;
		}

		/// <summary>
		/// Dispose implementation
		/// </summary>
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
		}
		#endregion
	}
}