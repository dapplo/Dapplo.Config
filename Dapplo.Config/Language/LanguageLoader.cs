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
using Dapplo.Config.Ini;
using Dapplo.Config.Support;

namespace Dapplo.Config.Language
{
	/// <summary>
	/// The language loader should be used to fill ILanguage proxy interfaces.
	/// It is possible to specify the directory locations, in order, where files with certain patterns should be located.
	/// </summary>
	public class LanguageLoader
	{
		private readonly Regex _cleanup = new Regex(@"[^a-zA-Z0-9]+");
		private readonly IDictionary<Type, IPropertyProxy> _languageConfigs = new Dictionary<Type, IPropertyProxy>();
		private readonly AsyncLock _asyncLock = new AsyncLock();
		private readonly IDictionary<string, string> _allProperties = new Dictionary<string, string>();
		private readonly string _applicationName;
		private readonly string _filePattern;
		private readonly IList<string> _files;
		private bool _initialReadDone;
		private readonly IDictionary<string, string> _availableLanguages = new Dictionary<string, string>();

		public LanguageLoader(string applicationName, string defaultLanguage = "en-US", string filePatern = @"language_([a-zA-Z]+-[a-zA-Z]+)\.ini")
		{
			CurrentLanguage = defaultLanguage;
			_filePattern = filePatern;
			_applicationName = applicationName;
			_files = ScanForFiles(true);
			_availableLanguages = (from filename in _files
				select Regex.Replace(Path.GetFileName(filename), _filePattern, "$1")).Distinct().ToDictionary(x => x, x => CultureInfo.GetCultureInfo(x).NativeName);
		}

		public string CurrentLanguage
		{
			get;
			private set;
		}

		public IDictionary<string, string> AvailableLanguages
		{
			get
			{
				return _availableLanguages;
			}
		}

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
						if (executingAssembly != null)
						{
							startupDirectory = Path.GetDirectoryName(executingAssembly.Location);
						}
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
				select Directory.GetFiles(path, "*.ini", SearchOption.AllDirectories).Where(f => Regex.IsMatch(Path.GetFileName(f), _filePattern)))
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
			var languageObject = (ILanguage) propertyProxy.PropertyObject;
            using (await _asyncLock.LockAsync().ConfigureAwait(false))
            {
                if (!_languageConfigs.ContainsKey(type))
				{
					if (!_initialReadDone)
					{
						await ReloadAsync(token).ConfigureAwait(false);
					}
					FillLanguageConfig(propertyProxy);
					_languageConfigs.Add(type, propertyProxy);
				}
			}

			return languageObject;
		}

		/// <summary>
		/// This is reloading all the .ini files, and will refill the language objects.
		/// </summary>
		public async Task ReloadAsync(CancellationToken token = default(CancellationToken))
		{
			var languageFiles = from file in _files
				where file.Contains(CurrentLanguage)
				select file;
			_allProperties.Clear();
			foreach (var languageFile in languageFiles)
			{
				var newIni = await IniFile.ReadAsync(languageFile, Encoding.UTF8, token).ConfigureAwait(false);
				foreach (var section in newIni.Keys)
				{
					var properties = newIni[section];
					foreach (var key in properties.Keys)
					{
						var cleanKey = _cleanup.Replace(string.Format("{0}{1}", section, key), "").ToLowerInvariant();
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
			foreach (PropertyInfo propertyInfo in propertyProxy.PropertyObjectType.GetProperties())
			{
				string key = _cleanup.Replace(string.Format("{0}{1}", prefix, propertyInfo.Name), "").ToLowerInvariant();
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