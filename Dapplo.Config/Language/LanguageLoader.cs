/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Ini;
using Dapplo.Config.Support;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Dapplo.Config.Language
{
	/// <summary>
	/// The language loader should be used to fill ILanguage proxy interfaces.
	/// It is possible to specify the directory locations, in order, where files with certain patterns should be located.
	/// </summary>
	public class LanguageLoader
	{
		private static readonly IDictionary<string, LanguageLoader> LoaderStore = new NonStrictLookup<LanguageLoader>();
		private readonly IDictionary<Type, IPropertyProxy> _languageTypeConfigs = new Dictionary<Type, IPropertyProxy>();
		private readonly IDictionary<string, ILanguage> _languageConfigs = new NonStrictLookup<ILanguage>();
		private readonly AsyncLock _asyncLock = new AsyncLock();
		private readonly IDictionary<string, IDictionary<string, string>> _allTranslations = new NonStrictLookup<IDictionary<string, string>>();
		private readonly string _applicationName;
		private readonly Regex _filePattern;
		private bool _initialReadDone;

		/// <summary>
		/// Static helper to retrieve the LanguageLoader that was created with the supplied parameters
		/// </summary>
		/// <param name="applicationName"></param>
		/// <returns>LanguageLoader</returns>
		public static LanguageLoader Get(string applicationName)
		{
			return LoaderStore[applicationName];
		}

		/// <summary>
		/// Static helper to retrieve the first LanguageLoader that was created
		/// </summary>
		/// <returns>LanguageLoader</returns>
		public static LanguageLoader Current
		{
			get
			{
				return LoaderStore.First().Value;
			}
		}

		/// <summary>
		/// Delete the Language objects for the specified application, mostly used in tests
		/// </summary>
		/// <param name="applicationName"></param>
		public static void Delete(string applicationName)
		{
			LoaderStore.Remove(applicationName);
		}

		/// <summary>
		/// Create a LanguageLoader, this is your container for all the ILanguage implementing interfaces.
		/// You can supply a default language right away.
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="defaultLanguage"></param>
		/// <param name="filePatern">Pattern for the filename, the ietf group needs to be in there!</param>
		/// <param name="checkStartupDirectory"></param>
		/// <param name="checkAppDataDirectory"></param>
		/// <param name="specifiedDirectories"></param>
		public LanguageLoader(string applicationName, string defaultLanguage = "en-US", string filePatern = @"language(_(?<module>[a-zA-Z0-9]*))?-(?<IETF>[a-zA-Z]{2}(-[a-zA-Z]+)?-[a-zA-Z]+)\.(ini|xml)", bool checkStartupDirectory = true, bool checkAppDataDirectory = true, ICollection<string> specifiedDirectories = null)
		{
			if (LoaderStore.ContainsKey(applicationName))
			{
				throw new InvalidOperationException($"{applicationName} was already created!");
			}
			CurrentLanguage = defaultLanguage;
			_filePattern = new Regex(filePatern, RegexOptions.Compiled);
			_applicationName = applicationName;
			ScanFiles(checkStartupDirectory, checkAppDataDirectory, specifiedDirectories);
			LoaderStore.SafelyAddOrOverwrite(applicationName, this);
		}

		/// <summary>
		/// Call this to make sure that all languages have translations.
		/// This will walk through the files of the supplied language (or the one with the most translations)
		/// and copy the "missing" files to the others. By doing this, all non translated components should be in this language.
		/// </summary>
		public void CorrectMissingTranslations()
		{
			var baseIetf = (from ietf in Files.Keys
					select new
					{
						ietf, Files[ietf].Count
					}).OrderByDescending(x => x.Count).First().ietf;

			var baseFileList = Files[baseIetf];
			foreach (var ietf in Files.Keys)
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

					if (!comparingFiles.Contains(possibleTargetFile))
					{
						// Add missing translation
						Files[ietf].Add(file);
                    }
				}
            }
		}

		/// <summary>
		/// Get the IETF of the current language.
		/// For the name of the language, use the AvailableLanguages with this value as the key.
		/// </summary>
		public string CurrentLanguage
		{
			get;
			private set;
		}

		/// <summary>
		/// All languages that were found in the files during the scan. 
		/// </summary>
		public IDictionary<string, string> AvailableLanguages
		{
			get;
			private set;
		}

		/// <summary>
		/// The files, ordered to the IETF, that were found during the scan
		/// </summary>
		public IDictionary<string, List<string>> Files
		{
			get;
			private set;
		}

		/// <summary>
		/// Change the language, this will only do something if the language actually changed.
		/// All files are reloaded. 
		/// </summary>
		/// <param name="ietf">The iso code for the language to use</param>
		/// <param name="token">CancellationToken for the loading</param>
		/// <returns>Task</returns>
		public async Task ChangeLanguage(string ietf, CancellationToken token = default(CancellationToken))
		{
			if (ietf == CurrentLanguage)
			{
				return;
			}
			if (AvailableLanguages.ContainsKey(ietf))
			{
				CurrentLanguage = ietf;
				await ReloadAsync(token).ConfigureAwait(false);
			}
		}


		/// <summary>
		/// Helper to create the location of a file
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

			Files = FileLocations.Scan(directories, _filePattern)
				.GroupBy(x => x.Item2.Groups["IETF"].Value)
				.ToDictionary(group => group.Key, group => group.Select(x => x.Item1)
				.ToList());

			var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
				.ToLookup(e => e.IetfLanguageTag, StringComparer.OrdinalIgnoreCase).ToDictionary(x=>x.Key, x=> x.First());

			//TODO: Create custom culture for all not available, see: https://msdn.microsoft.com/en-us/library/ms172469(v=vs.90).aspx

			AvailableLanguages = (from ietf in Files.Keys
								  where allCultures.ContainsKey(ietf)
								  select ietf).Distinct().ToDictionary(ietf => ietf, ietf => allCultures[ietf].NativeName);
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <typeparam name="T">Your property interface, which extends IIniSection</typeparam>
		/// <returns>instance of type T</returns>
		public async Task<T> RegisterAndGetAsync<T>(CancellationToken token = default(CancellationToken)) where T : ILanguage
		{
			return (T)await RegisterAndGetAsync(typeof(T), token).ConfigureAwait(false);
		}

		/// <summary>
		/// Register the supplied types
		/// </summary>
		/// <param name="types">Types to register, these must extend ILanguage</param>
		/// <param name="token"></param>
		/// <returns>List with instances for the supplied types</returns>
		public async Task<IList<ILanguage>> RegisterAndGetAsync(IEnumerable<Type> types, CancellationToken token = default(CancellationToken))
		{
			IList<ILanguage> languageTypes = new List<ILanguage>();
			foreach (var type in types)
			{
				languageTypes.Add(await RegisterAndGetAsync(type, token).ConfigureAwait(false));
			}
			return languageTypes;
		}

		/// <summary>
		/// Register a Property Interface to this language loader, this method will return the filled property object 
		/// </summary>
		/// <param name="type">Type to register, this must extend ILanguage</param>
		/// <param name="token"></param>
		/// <returns>instance of type</returns>
		public async Task<ILanguage> RegisterAndGetAsync(Type type, CancellationToken token = default(CancellationToken))
		{
			if (!typeof(ILanguage).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a ILanguage");
			}
			var propertyProxy = ProxyBuilder.GetOrCreateProxy(type);
			var languageObject = (ILanguage)propertyProxy.PropertyObject;
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				if (!_languageTypeConfigs.ContainsKey(type))
				{
					_languageTypeConfigs.Add(type, propertyProxy);
					_languageConfigs.Add(GetPrefix(propertyProxy), languageObject);
					if (!_initialReadDone)
					{
						await ReloadAsync(token).ConfigureAwait(false);
					}
					else
					{
						FillLanguageConfig(propertyProxy);
					}
				}
			}

			return languageObject;
		}

		/// <summary>
		/// Register a Property Interface to this language loader, this method will return the filled property object 
		/// </summary>
		/// <param name="type">Type to register, this must extend ILanguage</param>
		/// <returns>instance of type</returns>
		public ILanguage RegisterAndGet(Type type)
		{
			if (!typeof(ILanguage).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a ILanguage");
			}
			var propertyProxy = ProxyBuilder.GetOrCreateProxy(type);
			var languageObject = (ILanguage)propertyProxy.PropertyObject;
			if (!_languageTypeConfigs.ContainsKey(type))
			{
				_languageTypeConfigs.Add(type, propertyProxy);
				_languageConfigs.Add(GetPrefix(propertyProxy), languageObject);
				FillLanguageConfig(propertyProxy);
			}

			return languageObject;
		}

		/// <summary>
		/// Get the specified ILanguage type
		/// </summary>
		/// <typeparam name="T">ILanguage</typeparam>
		/// <returns>T</returns>
		public T Get<T>() where T : ILanguage
		{
			return (T)Get(typeof(T));
		}

		/// <summary>
		/// Get the specified ILanguage type
		/// </summary>
		/// <param name="type">ILanguage to look for</param>
		/// <returns>ILanguage</returns>
		public ILanguage Get(Type type)
		{
			if (!typeof(ILanguage).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a ILanguage");
			}
			if (!_initialReadDone)
			{
				throw new InvalidOperationException("Please load before retrieving the language");
			}
			return (ILanguage)ProxyBuilder.GetProxy(type).PropertyObject;
		}

		/// <summary>
		/// Get the specified ILanguage type
		/// </summary>
		/// <param name="prefix">ILanguage prefix to look for</param>
		/// <returns>ILanguage</returns>
		public ILanguage this[string prefix]
		{
			get
			{
				return _languageConfigs[prefix];
			}
		}

		/// <summary>
		/// This is reloading all the .ini files, and will refill the language objects.
		/// </summary>
		public async Task ReloadAsync(CancellationToken token = default(CancellationToken))
		{
			_allTranslations.Clear();
			if (Files.ContainsKey(CurrentLanguage))
			{
				foreach (var languageFile in Files[CurrentLanguage])
				{
					IDictionary<string, IDictionary<string, string>> newResources;
					if (languageFile.EndsWith(".ini"))
					{
						newResources = await IniFile.ReadAsync(languageFile, Encoding.UTF8, token).ConfigureAwait(false);
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
					foreach (var section in newResources.Keys)
					{
						var properties = newResources[section];
						IDictionary<string, string> sectionTranslations;
						if (!_allTranslations.TryGetValue(section, out sectionTranslations))
						{
							sectionTranslations = new Dictionary<string, string>();
							_allTranslations.Add(section, sectionTranslations);
						}
						foreach (var key in properties.Keys)
						{
							sectionTranslations.SafelyAddOrOverwrite(key, properties[key]);
						}
					}
				}
			}
			_initialReadDone = true;

			// Reset the sections that have already been registered
			foreach (var proxy in _languageTypeConfigs.Values)
			{
				FillLanguageConfig(proxy);
			}
		}

		/// <summary>
		/// Read the resources from the specified file
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
					group resourceElement by resourcesElement.Attribute("prefix").Value into resourceElementGroup
					select resourceElementGroup).ToDictionary(group => @group.Key, group => (IDictionary<string, string>)@group.ToDictionary(x => x.Attribute("name").Value, x => x.Value.Trim()));
		}

		/// <summary>
		/// Retrieve the language prefix from the IPropertyProxy
		/// </summary>
		/// <param name="propertyProxy"></param>
		/// <returns>string</returns>
		private string GetPrefix(IPropertyProxy propertyProxy)
		{
			string prefix = "";
			var languageAttribute = propertyProxy.PropertyObjectType.GetCustomAttribute<LanguageAttribute>();
			if (languageAttribute != null)
			{
				prefix = languageAttribute.Prefix;
			}
			return prefix;
		}

		/// <summary>
		/// Fill the backing properties of the supplied proxy-object.
		/// Match the ini-file properties with the name of the property.
		/// </summary>
		/// <param name="propertyProxy"></param>
		private void FillLanguageConfig(IPropertyProxy propertyProxy)
		{
			var prefix = GetPrefix(propertyProxy);
			var propertyObject = (ILanguage)propertyProxy.PropertyObject;
			IDictionary<string, string> sectionTranslations;
			if (!_allTranslations.TryGetValue(prefix, out sectionTranslations))
			{
				// No values, reset all
				foreach (var propertyInfo in propertyProxy.AllPropertyInfos.Values)
				{
					propertyObject.RestoreToDefault(propertyInfo.Name);
                }
				return;
			}

			foreach (var propertyInfo in propertyProxy.AllPropertyInfos.Values)
			{
				var key = propertyInfo.Name;
                string translation;
				if (sectionTranslations.TryGetValue(key, out translation))
				{
					propertyProxy.Set(key, translation);
					sectionTranslations.Remove(key);
				}
				else
				{
					propertyObject.RestoreToDefault(key);
				}
			}

			// Add all unprocessed values
			foreach (var key in sectionTranslations.Keys)
			{
				propertyProxy.Properties.SafelyAddOrOverwrite(key, sectionTranslations[key]);

			}
		}
	}
}