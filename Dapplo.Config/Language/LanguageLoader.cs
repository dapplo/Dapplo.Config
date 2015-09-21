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
		private readonly Regex _propertyCleanup = new Regex(@"[^a-zA-Z0-9]+", RegexOptions.Compiled);
		private readonly IDictionary<Type, IPropertyProxy> _languageConfigs = new Dictionary<Type, IPropertyProxy>();
		private readonly AsyncLock _asyncLock = new AsyncLock();
		private readonly IDictionary<string, string> _allProperties = new Dictionary<string, string>();
		private static readonly IDictionary<string, LanguageLoader> LoaderStore = new Dictionary<string, LanguageLoader>();
		private readonly string _applicationName;
		private readonly Regex _filePattern;
		private readonly IList<string> _files;
		private bool _initialReadDone;
		private readonly IDictionary<string, string> _availableLanguages;

		/// <summary>
		/// Static helper to retrieve the LanguageLoader that was created with the supplied parameters
		/// </summary>
		/// <param name="applicationName"></param>
		/// <returns>LanguageLoader</returns>
		public static LanguageLoader Get(string applicationName) {
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
		/// Create a LanguageLoader, this is your container for all the ILanguage implementing interfaces.
		/// You can supply a default language right away.
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="defaultLanguage"></param>
		/// <param name="filePatern">Pattern for the filename, the ietf group needs to be in there!</param>
		public LanguageLoader(string applicationName, string defaultLanguage = "en-US", string filePatern = @"language(_(?<module>[a-zA-Z0-9]*))?-(?<IETF>[a-zA-Z]{2}(-[a-zA-Z]+)?-[a-zA-Z]+)\.(ini|xml)")
		{
			CurrentLanguage = defaultLanguage;
			_filePattern = new Regex(filePatern, RegexOptions.Compiled);
			_applicationName = applicationName;
			_files = ScanForFiles(true);
			_availableLanguages = (
				from filename
				in _files
				select _filePattern.Match(Path.GetFileName(filename)).Groups["IETF"].Value)
				.Distinct()
				.Where(x => SavelyGetCultureInfo(x) != null)
				.ToDictionary(x => x, x => CultureInfo.GetCultureInfo(x).NativeName
			);
			LoaderStore.SafelyAddOrOverwrite(applicationName, this);
		}

		/// <summary>
		/// Try to get the GetCultureInfo, return null if this is not available
		/// </summary>
		/// <param name="ietf"></param>
		/// <returns></returns>
		private CultureInfo SavelyGetCultureInfo(string ietf)
		{
			try
			{
                return CultureInfo.GetCultureInfo(ietf);
            } catch {
				Console.WriteLine(ietf);
			}
			return null;
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
			get
			{
				return _availableLanguages;
			}
		}

		/// <summary>
		/// Get the list of the files which were found during the scan
		/// </summary>
		public IList<string> Files
		{
			get
			{
				return _files;
			}
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
			if (_availableLanguages.ContainsKey(ietf))
			{
				CurrentLanguage = ietf;
				await ReloadAsync(token).ConfigureAwait(false);
			}
		}

	
		/// <summary>
		/// Helper to create the location of a file
		/// </summary>
		/// <param name="checkStartupDirectory"></param>
		/// <param name="specifiedDirectory"></param>
		/// <returns>all language files</returns>
		private IList<string> ScanForFiles(bool checkStartupDirectory, string specifiedDirectory = null)
		{
			IList<string> directories = new List<string>();
			if (specifiedDirectory != null)
			{
				directories.Add(specifiedDirectory);
			}
			else
			{
				if (checkStartupDirectory)
				{
					var entryAssembly = Assembly.GetEntryAssembly();
					string startupDirectory = null;
					if (entryAssembly != null)
					{
						startupDirectory = Path.GetDirectoryName(entryAssembly.Location);
					}
					else
					{
						var executingAssembly = Assembly.GetExecutingAssembly();
						startupDirectory = Path.GetDirectoryName(executingAssembly.Location);
					}
					if (startupDirectory != null)
					{
						directories.Add(Path.Combine(startupDirectory, "languages"));
					}
				}
				string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _applicationName);
				directories.Add(Path.Combine(appDataDirectory, "languages"));
			}
			var files = (from path in directories
				where Directory.Exists(path)
				select Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(f => _filePattern.IsMatch(Path.GetFileName(f))))
// See: https://youtrack.jetbrains.com/issue/RSRP-413613
// ReSharper disable once PossibleMultipleEnumeration
				.SelectMany(i => i).ToList();
			return files;
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <typeparam name="T">Your property interface, which extends IIniSection</typeparam>
		/// <returns>instance of type T</returns>
		public async Task<T> RegisterAndGetAsync<T>(CancellationToken token = default(CancellationToken)) where T : ILanguage
		{
			return (T) await RegisterAndGetAsync(typeof (T), token).ConfigureAwait(false);
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
			if (!typeof (ILanguage).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a ILanguage");
			}
			var propertyProxy = ProxyBuilder.GetOrCreateProxy(type);
			var languageObject = (ILanguage)propertyProxy.PropertyObject;
            using (await _asyncLock.LockAsync().ConfigureAwait(false))
            {
                if (!_languageConfigs.ContainsKey(type))
				{
					_languageConfigs.Add(type, propertyProxy);
					if (!_initialReadDone)
					{
						await ReloadAsync(token).ConfigureAwait(false);
					}
					FillLanguageConfig(propertyProxy);
				}
			}

			return languageObject;
		}

		/// <summary>
		/// Get the specified ILanguage type
		/// </summary>
		/// <typeparam name="T">ILanguage</typeparam>
		/// <returns>T</returns>
		public T Get<T>() where T : ILanguage {
			return (T)Get(typeof(T));
		}

		/// <summary>
		/// Get the specified ILanguage type
		/// </summary>
		/// <param name="type">ILanguage to look for</param>
		/// <returns>ILanguage</returns>
		public ILanguage Get(Type type) {
			if (!typeof(ILanguage).IsAssignableFrom(type)) {
				throw new ArgumentException("type is not a ILanguage");
			}
			if (!_initialReadDone) {
				throw new InvalidOperationException("Please load before retrieving the language");
			}
			var propertyProxy = ProxyBuilder.GetProxy(type);
			var languageObject = (ILanguage)propertyProxy.PropertyObject;
			return languageObject;
		}

		/// <summary>
		/// This is reloading all the .ini files, and will refill the language objects.
		/// </summary>
		public async Task ReloadAsync(CancellationToken token = default(CancellationToken))
		{
			var languageFiles =
				from file
				in _files
				where file.Contains(CurrentLanguage)
				select file;

			_allProperties.Clear();

			foreach (var languageFile in languageFiles)
			{
				IDictionary<string, IDictionary<string, string>> newResources;
				if (languageFile.EndsWith(".ini")) {
					newResources = await IniFile.ReadAsync(languageFile, Encoding.UTF8, token).ConfigureAwait(false);
				} else if (languageFile.EndsWith(".xml")) {
					newResources =
						(from resourcesElement in XDocument.Load(languageFile).Root.Elements("resources")
						 where resourcesElement.Attribute("prefix") != null
						 from resourceElement in resourcesElement.Elements("resource")
						 group resourceElement by resourcesElement.Attribute("prefix").Value into resourceElementGroup
						 select resourceElementGroup).ToDictionary(group => group.Key, group => (IDictionary<string,string>)group.ToDictionary(x => x.Attribute("name").Value, x => x.Value));
				} else {
					throw new NotSupportedException(string.Format("Can't read the file format for {0}", languageFile));
				}
				foreach (var section in newResources.Keys) {
					var properties = newResources[section];
					foreach (var key in properties.Keys) {
						var cleanKey = _propertyCleanup.Replace(string.Format("{0}{1}", section, key), "").ToLowerInvariant();
						_allProperties.SafelyAddOrOverwrite(cleanKey, properties[key]);
					}
				}
			}
			_initialReadDone = true;

			// Reset the sections that have already been registered
			FillLanguageConfigs();
		}

		private void FillLanguageConfigs()
		{
			foreach (var proxy in _languageConfigs.Values)
			{
				FillLanguageConfig(proxy);
			}
		}

		/// <summary>
		/// Fill the backing properties of the supplied proxy-object.
		/// Match the ini-file properties with the name of the property.
		/// </summary>
		/// <param name="propertyProxy"></param>
		private void FillLanguageConfig(IPropertyProxy propertyProxy)
		{
			var languageAttribute = propertyProxy.PropertyObjectType.GetCustomAttribute<LanguageAttribute>();
			string prefix = "";
			if (languageAttribute != null)
			{
				prefix = languageAttribute.Prefix;
			}

			var propertyObject = (ILanguage) propertyProxy.PropertyObject;
			foreach (PropertyInfo propertyInfo in propertyProxy.AllPropertyInfos)
			{
				string key = _propertyCleanup.Replace(string.Format("{0}{1}", prefix, propertyInfo.Name), "").ToLowerInvariant();
				string translation;
				if (_allProperties.TryGetValue(key, out translation))
				{
					propertyProxy.Set(propertyInfo.Name, translation);
				}
				else
				{
					propertyObject.RestoreToDefault(propertyInfo.Name);
				}
			}
		}
	}
}