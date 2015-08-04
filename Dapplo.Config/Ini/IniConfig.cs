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

using Dapplo.Config.Converters;
using Dapplo.Config.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// The IniConfig is used to bind IIniSection proxy objects to an ini file.
	/// </summary>
	public class IniConfig
	{
		private readonly string _fileName;
		private readonly string _applicationName;
		private const string Defaults = "-defaults";
		private const string Constants = "-constants";
		private const string IniExtension = "ini";
		private static IDictionary<string, IniConfig> ConfigStore = new Dictionary<string, IniConfig>();
		private readonly AsyncLock _asyncLock = new AsyncLock();
		private readonly string _iniFile;
		private readonly string _fixedDirectory;
		private readonly IDictionary<string, IIniSection> _iniSections = new SortedDictionary<string, IIniSection>();
		private bool _initialReadDone;
		private IDictionary<Type, Action<IIniSection>> _afterLoadActions = new Dictionary<Type, Action<IIniSection>>();
		private IDictionary<Type, Action<IIniSection>> _beforeSaveActions = new Dictionary<Type, Action<IIniSection>>();
		private IDictionary<Type, Action<IIniSection>> _afterSaveActions = new Dictionary<Type, Action<IIniSection>>();
		private IDictionary<string, IDictionary<string, string>> _defaults;
		private IDictionary<string, IDictionary<string, string>> _constants;
		private IDictionary<string, IDictionary<string, string>> _ini = new SortedDictionary<string, IDictionary<string, string>>();

		private readonly IDictionary<Type, Type> _converters = new Dictionary<Type, Type>();

		/// <summary>
		/// Assign your own error handler to get all the write errors
		/// </summary>
		public Action<IIniSection, IniValue, Exception> WriteErrorHandler
		{
			get;
			set;
		}

		/// <summary>
		/// Assign your own error handler to get all the read errors
		/// </summary>
		public Action<IIniSection, IniValue, Exception> ReadErrorHandler
		{
			get;
			set;
		}

		/// <summary>
		/// Location of the file where the ini config is stored
		/// </summary>
		public string IniLocation
		{
			get
			{
				return _iniFile;
			}
		}

		/// <summary>
		/// Static helper to remove the IniConfig from the store.
		/// This is interal, as it normally should not be used.
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="fileName"></param>
		public static void Delete(string applicationName, string fileName)
		{
			ConfigStore.Remove(string.Format("{0}.{1}", applicationName, fileName));
		}

		/// <summary>
		/// Static helper to retrieve the IniConfig that was created with the supplied parameters
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="fileName"></param>
		/// <returns>IniConfig</returns>
		public static IniConfig Get(string applicationName, string fileName)
		{
			return ConfigStore[string.Format("{0}.{1}", applicationName, fileName)];
		}

		/// <summary>
		/// Setup the management of an .ini file location
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="fileName"></param>
		/// <param name="fixedDirectory">Specify a path if you don't want to use the default loading</param>
		/// <param name="registerDefaultConverters">false if you don't want to have any default converters</param>
		public IniConfig(string applicationName, string fileName, string fixedDirectory = null)
		{
			_applicationName = applicationName;
			_fileName = fileName;
			_fixedDirectory = fixedDirectory;
			// Look for the ini file, this is only done 1 time.
			_iniFile = CreateFileLocation(false, "", _fixedDirectory);

			WriteErrorHandler = (iniSection, iniValue, exception) =>
			{
				if (!iniValue.Behavior.IgnoreErrors)
				{
					throw exception;
				}
			};

			ReadErrorHandler = (iniSection, iniValue, exception) =>
			{
				if (!iniValue.Behavior.IgnoreErrors)
				{
					throw exception;
				}
			};
			ConfigStore.Add(string.Format("{0}.{1}", applicationName, fileName), this);
		}

		/// <summary>
		/// Set the default converter for the specified type
		/// </summary>
		/// <param name="type"></param>
		/// <param name="typeConverter"></param>
		public IniConfig SetDefaultConverter(Type type, Type typeConverter)
		{
			_converters.SafelyAddOrOverwrite(type, typeConverter);
			return this;
		}

		/// <summary>
		/// Set the after load action for a IIniSection
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="afterLoadAction"></param>
		/// <returns></returns>
		public IniConfig AfterLoad<T>(Action<T> afterLoadAction) where T : IIniSection
		{
			_afterLoadActions.SafelyAddOrOverwrite(typeof(T), new Action<IIniSection>((section) => afterLoadAction((T)section)));
			return this;
		}

		/// <summary>
		/// Set before save action for a IIniSection, this can be used to chance values before they are stored
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="beforeSaveAction"></param>
		/// <returns></returns>
		public IniConfig BeforeSave<T>(Action<T> beforeSaveAction)
		{
			_beforeSaveActions.SafelyAddOrOverwrite(typeof(T), new Action<IIniSection>((section) => beforeSaveAction((T)section)));
			return this;
		}


		/// <summary>
		/// Set after save action for a IIniSection, this can be used to change value changed in before save back, after saving
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="afterSaveAction"></param>
		/// <returns>this</returns>
		public IniConfig AfterSave<T>(Action<T> afterSaveAction)
		{
			_afterSaveActions.SafelyAddOrOverwrite(typeof(T), new Action<IIniSection>((section) => afterSaveAction((T)section)));
			return this;
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <typeparam name="T">Your property interface, which extends IIniSection</typeparam>
		/// <returns>instance of type T</returns>
		public async Task<T> RegisterAndGetAsync<T>(CancellationToken token = default(CancellationToken)) where T : IIniSection
		{
			return (T)await RegisterAndGetAsync(typeof(T), token).ConfigureAwait(false);
		}

		/// <summary>
		/// Register the supplied types
		/// </summary>
		/// <param name="types">Types to register, these must extend IIniSection</param>
		/// <param name="token"></param>
		/// <returns>List with instances for the supplied types</returns>
		public async Task<IList<IIniSection>> RegisterAndGetAsync(IEnumerable<Type> types, CancellationToken token = default(CancellationToken))
		{
			IList<IIniSection> sections = new List<IIniSection>();
			foreach (var type in types)
			{
				sections.Add(await RegisterAndGetAsync(type, token).ConfigureAwait(false));
			}
			return sections;
		}

		/// <summary>
		/// Get the specified ini type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns>T</returns>
		public T Get<T>() where T : IIniSection
		{
			return (T)Get(typeof(T));
		}

		/// <summary>
		/// Get the specified IIniSection type
		/// </summary>
		/// <param name="type">IIniSection to look for</param>
		/// <returns>IIniSection</returns>
		public IIniSection Get(Type type)
		{
			if (!typeof(IIniSection).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a IIniSection");
			}
			if (!_initialReadDone) {
				throw new InvalidOperationException("Please load before retrieving the ini-sections");
			}
			var propertyProxy = ProxyBuilder.GetProxy(type);
			var iniSection = (IIniSection)propertyProxy.PropertyObject;
			return iniSection;
		}

		/// <summary>
		/// A simple get by name for the IniSection
		/// </summary>
		/// <param name="sectionName"></param>
		/// <returns>IIniSection</returns>
		public IIniSection Get(string sectionName) {
			return _iniSections[sectionName];
		}

		/// <summary>
		/// A simple try get by name for the IniSection
		/// </summary>
		/// <param name="sectionName">Name of the section</param>
		/// <param name="section">out parameter with the IIniSection</param>
		/// <returns>bool with true if it worked</returns>
		public bool TryGet(string sectionName, out IIniSection section) {
			return _iniSections.TryGetValue(sectionName, out section);
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <param name="type">Type to register, this must extend IIniSection</param>
		/// <param name="token"></param>
		/// <returns>instance of type</returns>
		public async Task<IIniSection> RegisterAndGetAsync(Type type, CancellationToken token = default(CancellationToken))
		{
			if (!typeof(IIniSection).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a IIniSection");
			}
			var propertyProxy = ProxyBuilder.GetOrCreateProxy(type);
			var iniSection = (IIniSection)propertyProxy.PropertyObject;
			var iniSectionName = iniSection.GetSectionName();

			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				if (_iniSections.ContainsKey(iniSectionName))
				{
					return iniSection;
				}
				// Add before loading, so it will be handled automatically
				_iniSections.Add(iniSectionName, iniSection);
				if (!_initialReadDone)
				{
					await ReloadInternalAsync(false, token).ConfigureAwait(false);
				}
				else
				{
					FillSection(iniSection);
				}
			}

			return iniSection;
		}

		/// <summary>
		/// Helper to create the location of a file
		/// </summary>
		/// <param name="checkStartupDirectory"></param>
		/// <param name="postfix"></param>
		/// <param name="specifiedDirectory"></param>
		/// <returns></returns>
		private string CreateFileLocation(bool checkStartupDirectory, string postfix = "", string specifiedDirectory = null)
		{
			string file = null;
			if (specifiedDirectory != null)
			{
				file = Path.Combine(specifiedDirectory, string.Format("{0}{1}.{2}", _fileName, postfix, IniExtension));
			}
			else
			{
				if (checkStartupDirectory)
				{
					var entryAssembly = Assembly.GetEntryAssembly();
					if (entryAssembly != null)
					{
						string startupDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
						if (startupDirectory != null)
						{
							file = Path.Combine(startupDirectory, string.Format("{0}{1}.{2}", _fileName, postfix, IniExtension));
						}
					}
				}
				if (file == null || !File.Exists(file))
				{
					string appDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _applicationName);
					file = Path.Combine(appDataDirectory, string.Format("{0}{1}.{2}", _fileName, postfix, IniExtension));
				}
			}
			return file;
		}

		/// <summary>
		/// Reset all the values, in all the registered ini sections, to their default
		/// </summary>
		public async Task ResetAsync(CancellationToken token = default(CancellationToken))
		{
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				foreach (var iniSection in _iniSections.Values)
				{
					foreach (var iniValue in iniSection.GetIniValues())
					{
						// TODO: Do we need to skip read/write protected values here?
						iniSection.RestoreToDefault(iniValue.PropertyName);
					}
					// Call the after load action
					Action<IIniSection> afterLoadAction;
					if (_afterLoadActions.TryGetValue(iniSection.GetType(), out afterLoadAction))
					{
						afterLoadAction(iniSection);
					}
				}
			}
		}

		/// <summary>
		/// Write the ini file
		/// </summary>
		public async Task WriteAsync(CancellationToken token = default(CancellationToken))
		{
			// Make sure only one write to file is running, other request will have to wait
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				string path = Path.GetDirectoryName(_iniFile);

				// Create the directory to write to, if it doesn't exist yet
				if (path != null && !Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				// Create the file as a stream
				using (var stream = new FileStream(_iniFile, FileMode.Create, FileAccess.Write))
				{
					// Write the registered ini sections to the stream
					await WriteToStreamInternalAsync(stream, token).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Store the ini to the supplied stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="token"></param>
		/// <returns>Task to await</returns>
		private async Task WriteToStreamInternalAsync(Stream stream, CancellationToken token = default(CancellationToken))
		{
			IDictionary<string, IDictionary<string, string>> iniSectionsComments = new SortedDictionary<string, IDictionary<string, string>>();

			// Loop over the "registered" sections
			foreach (var iniSection in _iniSections.Values)
			{
				Action<IIniSection> beforeSaveAction;
				if (_beforeSaveActions.TryGetValue(iniSection.GetType(), out beforeSaveAction))
				{
					beforeSaveAction(iniSection);
				}
				try
				{
					CreateSaveValues(iniSection, iniSectionsComments);
				}
				finally
				{
					// Eventually set the values back
					Action<IIniSection> afterSaveAction;
					if (_beforeSaveActions.TryGetValue(iniSection.GetType(), out afterSaveAction))
					{
						afterSaveAction(iniSection);
					}

				}
			}
			await IniFile.WriteAsync(stream, Encoding.UTF8, _ini, iniSectionsComments, token).ConfigureAwait(false);
			await stream.FlushAsync(token).ConfigureAwait(false);
		}

		/// <summary>
		/// Write all the IIniSections to the stream, this is also used for testing
		/// </summary>
		/// <param name="stream">Stream to write to</param>
		/// <param name="token"></param>
		/// <returns>Task</returns>
		public async Task WriteToStreamAsync(Stream stream, CancellationToken token = default(CancellationToken))
		{
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				await WriteToStreamInternalAsync(stream, token);
			}
		}

		/// <summary>
		/// Helper method to create ini section values for writing.
		/// The actual values are stored in the _ini
		/// </summary>
		/// <param name="iniSection">Section to write</param>
		/// <param name="iniSectionsComments">Comments</param>
		private void CreateSaveValues(IIniSection iniSection, IDictionary<string, IDictionary<string, string>> iniSectionsComments)
		{
			// This flag tells us if the header for the section is already written
			bool isSectionCreated = false;

			IDictionary<string, string> sectionProperties = new SortedDictionary<string, string>();
			IDictionary<string, string> sectionComments = new SortedDictionary<string, string>();
			// Loop over the ini values, this automatically skips all NonSerialized properties
			foreach (var iniValue in iniSection.GetIniValues())
			{
				// Check if we need to write the value, this is not needed when it has the default or if write is disabled
				if (!iniValue.IsWriteNeeded)
				{
					continue;
				}

				// Before we are going to write, we need to check if the section header "[Sectionname]" is already written.
				// If not, do so now before writing the properties of the section itself
				if (!isSectionCreated)
				{
					if (_ini.ContainsKey(iniSection.GetSectionName()))
					{
						_ini.Remove(iniSection.GetSectionName());
					}
					_ini.Add(iniSection.GetSectionName(), sectionProperties);
					iniSectionsComments.Add(iniSection.GetSectionName(), sectionComments);

					string description = iniSection.GetSectionDescription();
					if (!string.IsNullOrEmpty(description))
					{
						sectionComments.Add(iniSection.GetSectionName(), description);
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
					var propertyDescription = TypeDescriptor.GetProperties(iniSection.GetType()).Find(iniValue.PropertyName, false);
					context = new TypeDescriptorContext(iniSection, propertyDescription);
				}
				// ReSharper disable once EmptyGeneralCatchClause
				catch
				{
					// Ignore any exceptions
				}

				// Check if a converter is specified
				TypeConverter converter = iniValue.Converter;
				// If not, use the default converter for the property type
				if (converter == null)
				{
					Type value;
					if (_converters.TryGetValue(iniValue.ValueType, out value))
					{
						converter = (TypeConverter)Activator.CreateInstance(value);
					}
					else
					{
						converter = TypeDescriptor.GetConverter(iniValue.ValueType);
					}
				}
				else if (converter.CanConvertTo(typeof(IDictionary<string, string>)))
				{
					try
					{
						// Convert the dictionary to a string,string variant.
						var dictionaryProperties = (IDictionary<string, string>)converter.ConvertTo(context, CultureInfo.CurrentCulture, iniValue.Value, typeof(IDictionary<string, string>));
						// Use this to build a separate "section" which is called "[section-propertyname]"
						string dictionaryIdentifier = string.Format("{0}-{1}", iniSection.GetSectionName(), iniValue.IniPropertyName);
						if (_ini.ContainsKey(dictionaryIdentifier))
						{
							_ini.Remove(dictionaryIdentifier);
						}
						_ini.Add(dictionaryIdentifier, dictionaryProperties);
						if (!string.IsNullOrWhiteSpace(iniValue.Description))
						{
							IDictionary<string, string> dictionaryComments = new SortedDictionary<string, string>();
							dictionaryComments.Add(dictionaryIdentifier, iniValue.Description);
							iniSectionsComments.Add(dictionaryIdentifier, dictionaryComments);
						}
					}
					catch (Exception ex)
					{
						WriteErrorHandler(iniSection, iniValue, ex);
					}
					continue;
				}

				try
				{
					// Convert the value to a string
					string writingValue;
					if (context != null)
					{
						writingValue = converter.ConvertToInvariantString(context, iniValue.Value);
					}
					else
					{
						writingValue = converter.ConvertToInvariantString(iniValue.Value);
					}
					// And write the value with the IniPropertyName (which does NOT have to be the property name) to the file
					sectionProperties.Add(iniValue.IniPropertyName, writingValue);
				}
				catch (Exception ex)
				{
					WriteErrorHandler(iniSection, iniValue, ex);
				}
			}

		}

		/// <summary>
		/// This is reloading all the .ini files, and will refill the sections.
		/// If reset = true, ALL setting are lost
		/// Otherwise only the properties in the files will overwrite your settings.
		/// Usually this should not directly be called, unless you know that the file was changed by an external process.
		/// </summary>
		public async Task ReloadAsync(bool reset = true, CancellationToken token = default(CancellationToken))
		{
			using (await _asyncLock.LockAsync().ConfigureAwait(false))
			{
				await ReloadInternalAsync(reset, token);
			}
		}

		/// <summary>
		/// This is reloading all the .ini files, and will refill the sections.
		/// If reset = true, ALL setting are lost
		/// Otherwise only the properties in the files will overwrite your settings.
		/// Usually this should not directly be called, unless you know that the file was changed by an external process.
		/// </summary>
		private async Task ReloadInternalAsync(bool reset = true, CancellationToken token = default(CancellationToken))
		{
			if (reset)
			{
				await ResetAsync(token).ConfigureAwait(false);
			}

			_defaults = await IniFile.ReadAsync(CreateFileLocation(true, Defaults, _fixedDirectory), Encoding.UTF8, token).ConfigureAwait(false);
			_constants = await IniFile.ReadAsync(CreateFileLocation(true, Constants, _fixedDirectory), Encoding.UTF8, token).ConfigureAwait(false);
			var newIni = await IniFile.ReadAsync(_iniFile, Encoding.UTF8, token).ConfigureAwait(false);
			if (newIni != null)
			{
				_ini = newIni;
			}
			_initialReadDone = true;

			// Reset the sections that have already been registered
			FillSections();
		}

		/// <summary>
		/// Internal method, use the supplied ini-sections & properties to fill the sectoins
		/// </summary>
		/// <returns></returns>
		private void FillSections()
		{
			foreach (var iniSection in _iniSections.Values)
			{
				FillSection(iniSection);
			}
		}

		/// <summary>
		/// Helper method to fill the values of one section
		/// </summary>
		/// <param name="iniSection"></param>
		private void FillSection(IIniSection iniSection)
		{
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

			// After loadd
			Action<IIniSection> afterLoadAction;
			if (_afterLoadActions.TryGetValue(iniSection.GetType(), out afterLoadAction))
			{
				afterLoadAction(iniSection);
			}
		}

		/// <summary>
		/// Put the values from the iniProperties to the proxied object
		/// </summary>
		/// <param name="iniSections"></param>
		/// <param name="iniSection"></param>
		private void FillSection(IDictionary<string, IDictionary<string, string>> iniSections, IIniSection iniSection)
		{
			IDictionary<string, string> iniProperties = null;
			// Might be null
			iniSections.TryGetValue(iniSection.GetSectionName(), out iniProperties);

			IEnumerable<IniValue> iniValues = (from iniValue in iniSection.GetIniValues()
											   where iniValue.Behavior.Read
											   select iniValue);

			foreach (var iniValue in iniValues)
			{
				string dictionaryIdentifier = string.Format("{0}-{1}", iniSection.GetSectionName(), iniValue.IniPropertyName);
				// If there are no properties, there might still be a separate section for a dictionary
				IDictionary<string, string> value;
				if (iniValue.Converter != null && iniSections.TryGetValue(dictionaryIdentifier, out value))
				{
					try
					{
						iniValue.Value = iniValue.Converter.ConvertFrom(value);
					}
					catch (Exception ex)
					{
						ReadErrorHandler(iniSection, iniValue, ex);
					}

					continue;
				}
				// Skip if the iniValue does not have a default value and there is nothing to set
				if (iniProperties == null)
				{
					continue;
				}
				string stringValue;
				// Skip values that don't have a property
				if (!iniProperties.TryGetValue(iniValue.IniPropertyName, out stringValue))
				{
					continue;
				}
				Type stringType = typeof(string);
				Type destinationType = iniValue.ValueType;
				if (iniValue.Converter != null && iniValue.Converter.CanConvertFrom(stringType))
				{
					try
					{
						iniValue.Value = iniValue.Converter.ConvertFrom(stringValue);
					}
					catch (Exception ex)
					{
						ReadErrorHandler(iniSection, iniValue, ex);
					}
					continue;
				}

				// use default converter
				Type converterType;
				if (_converters.TryGetValue(iniValue.ValueType, out converterType))
				{
					var converter = (TypeConverter)Activator.CreateInstance(converterType);
					try
					{
						iniValue.Value = converter.ConvertFrom(stringValue);
					}
					catch (Exception ex)
					{
						ReadErrorHandler(iniSection, iniValue, ex);
					}
					continue;
				}

				if (destinationType != stringType)
				{
					var converter = TypeDescriptor.GetConverter(destinationType);
					if (converter.CanConvertFrom(stringType))
					{
						try
						{
							iniValue.Value = converter.ConvertFrom(stringValue);
						}
						catch (Exception ex)
						{
							ReadErrorHandler(iniSection, iniValue, ex);
						}
						continue;
					}
				}
				// just set it and hope it can be cast
				iniValue.Value = stringValue;
			}
		}

		/// <summary>
		/// Initialize the IniConfig by reading all the properties from the stream
		/// If this is called directly after construction, no files will be read which is useful for testing!
		/// </summary>
		public async Task ReadFromStreamAsync(Stream stream, CancellationToken token = default(CancellationToken))
		{
			_initialReadDone = true;
			// This is for testing, clear all defaults & constants as the 
			_defaults = null;
			_constants = null;
			_ini = await IniFile.ReadAsync(stream, Encoding.UTF8, token).ConfigureAwait(false);

			// Reset the current sections
			FillSections();
		}
	}
}