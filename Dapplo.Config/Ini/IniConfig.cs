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
		private readonly SemaphoreSlim _sync = new SemaphoreSlim(1);

		private readonly string _iniFile;
		private readonly string _fixedDirectory;
		private readonly IDictionary<string, IIniSection> _sections = new SortedDictionary<string, IIniSection>();
		private bool _initialReadDone;
		private IDictionary<string, IDictionary<string, string>> _defaults;
		private IDictionary<string, IDictionary<string, string>> _constants;
		private IDictionary<string, IDictionary<string, string>> _ini = new SortedDictionary<string, IDictionary<string, string>>();

		/// <summary>
		/// Setup the management of an .ini file location
		/// </summary>
		/// <param name="applicationName"></param>
		/// <param name="fileName"></param>
		/// <param name="fixedDirectory">Specify a path if you don't want to use the default loading</param>
		public IniConfig(string applicationName, string fileName, string fixedDirectory = null)
		{
			_applicationName = applicationName;
			_fileName = fileName;
			_fixedDirectory = fixedDirectory;
			// Look for the ini file, this is only done 1 time.
			_iniFile = CreateFileLocation(false, "", _fixedDirectory);
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <typeparam name="T">Your property interface, which extends IIniSection</typeparam>
		/// <returns>instance of type T</returns>
		public async Task<T> RegisterAndGetAsync<T>(CancellationToken token = default(CancellationToken)) where T : IIniSection
		{
			return (T)await RegisterAndGetAsync(typeof(T), token);
		}

		/// <summary>
		/// Register the supplied types
		/// </summary>
		/// <param name="types">Types to register, these must extend IIniSection</param>
		/// <returns>List with instances for the supplied types</returns>
		public async Task<IList<IIniSection>> RegisterAndGetAsync(IEnumerable<Type> types, CancellationToken token = default(CancellationToken))
		{
			IList<IIniSection> sections = new List<IIniSection>();
			foreach (var type in types)
			{
				sections.Add(await RegisterAndGetAsync(type, token));
			}
			return sections;
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <param name="type">Type to register, this must extend IIniSection</param>
		/// <returns>instance of type</returns>
		public async Task<IIniSection> RegisterAndGetAsync(Type type, CancellationToken token = default(CancellationToken))
		{
			if (!typeof(IIniSection).IsAssignableFrom(type))
			{
				throw new ArgumentException("type is not a IIniSection");
			}
			var _propertyProxy = ProxyBuilder.GetOrCreateProxy(type);
			var section = (IIniSection)_propertyProxy.PropertyObject;
			var sectionName = section.GetSectionName();

			using (await Sync.Wait(_sync))
			{
				if (!_sections.ContainsKey(sectionName))
				{
					if (!_initialReadDone)
					{
						await ReloadAsync(false, token);
					}
					FillSection(section);
					_sections.Add(sectionName, section);
				}
			}

			return section;
		}

		/// <summary>
		/// Helper to create the location of a file
		/// </summary>
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
						file = Path.Combine(startupDirectory, string.Format("{0}{1}.{2}", _fileName, postfix, IniExtension));
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
			using (await Sync.Wait(_sync))
			{
				foreach (var section in _sections.Values)
				{
					foreach (var iniValue in section.GetIniValues())
					{
						// TODO: Do we need to skip read/write protected values here?
						section.RestoreToDefault(iniValue.PropertyName);
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
			using (await Sync.Wait(_sync))
			{
				string path = Path.GetDirectoryName(_iniFile);

				// Create the directory to write to, if it doesn't exist yet
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}

				// Create the file as a stream
				using (var stream = new FileStream(_iniFile, FileMode.Create, FileAccess.Write))
				{
					// Write the registered ini sections to the stream
					await WriteToStreamAsync(stream);
				}
			}
		}

		/// <summary>
		/// Write all the IIniSections to the stream, this is also used for testing
		/// </summary>
		/// <param name="stream">Stream to write to</param>
		/// <returns>Task</returns>
		public async Task WriteToStreamAsync(Stream stream, CancellationToken token = default(CancellationToken))
		{
			IDictionary<string, IDictionary<string, string>> sectionsComments = new SortedDictionary<string, IDictionary<string, string>>();

			// Loop over the "registered" sections
			foreach (var section in _sections.Values)
			{
				// This flag tells us if the header for the section is already written
				bool isSectionCreated = false;

				IDictionary<string, string> sectionProperties = new SortedDictionary<string, string>();
				IDictionary<string, string> sectionComments = new SortedDictionary<string, string>();
				// Loop over the ini values, this automatically skips all NonSerialized properties
				foreach (var iniValue in section.GetIniValues())
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
						if (_ini.ContainsKey(section.GetSectionName()))
						{
							_ini.Remove(section.GetSectionName());
						}
						_ini.Add(section.GetSectionName(), sectionProperties);
						sectionsComments.Add(section.GetSectionName(), sectionComments);

						string description = section.GetSectionDescription();
						if (!string.IsNullOrEmpty(description))
						{
							sectionComments.Add(section.GetSectionName(), description);
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
						var propertyDescription = TypeDescriptor.GetProperties(section.GetType()).Find(iniValue.PropertyName, false);
						context = new TypeDescriptorContext(section, propertyDescription);
					}
					catch
					{
						// Ignore any exceptions
					}

					// Check if a converter is specified
					TypeConverter converter = iniValue.Converter;
					// If not, use the default converter for the property type
					if (converter == null)
					{
						converter = TypeDescriptor.GetConverter(iniValue.ValueType);
					}
					else if (converter.CanConvertTo(typeof(IDictionary<string, string>)))
					{
						try
						{
							// Convert the dictionary to a string,string variant.
							IDictionary<string, string> dictionaryProperties = (IDictionary<string, string>)converter.ConvertTo(context, CultureInfo.CurrentCulture, iniValue.Value, typeof(IDictionary<string, string>));
							// Use this to build a separate "section" which is called "[section-propertyname]"
							string dictionaryIdentifier = string.Format("{0}-{1}", section.GetSectionName(), iniValue.IniPropertyName);
							if (_ini.ContainsKey(dictionaryIdentifier))
							{
								_ini.Remove(dictionaryIdentifier);
							}
							_ini.Add(dictionaryIdentifier, dictionaryProperties);
							if (!string.IsNullOrWhiteSpace(iniValue.Description))
							{
								IDictionary<string, string> dictionaryComments = new SortedDictionary<string, string>();
								dictionaryComments.Add(dictionaryIdentifier, iniValue.Description);
								sectionsComments.Add(dictionaryIdentifier, dictionaryComments);
							}
						}
						catch
						{
							if (!iniValue.Behavior.IgnoreErrors)
							{
								throw;
							}
						}
						continue;
					}

					try
					{
						// Convert the value to a string
						var writingValue = converter.ConvertToInvariantString(context, iniValue.Value);
						// And write the value with the IniPropertyName (which does NOT have to be the property name) to the file
						sectionProperties.Add(iniValue.IniPropertyName, writingValue);
					}
					catch
					{
						if (!iniValue.Behavior.IgnoreErrors)
						{
							throw;
						}
					}

				}
			}
			await IniFile.WriteAsync(stream, Encoding.UTF8, _ini, sectionsComments, token);
		}

		/// <summary>
		/// This is reloading all the .ini files, and will refill the sections.
		/// If reset = true, ALL setting are lost
		/// Otherwise only the properties in the files will overwrite your settings.
		/// Usually this should not directly be called, unless you know that the file was changed by an external process.
		/// </summary>
		public async Task ReloadAsync(bool reset = true, CancellationToken token = default(CancellationToken))
		{
			if (reset)
			{
				await ResetAsync(token);
			}

			_defaults = await IniFile.ReadAsync(CreateFileLocation(true, Defaults, _fixedDirectory), Encoding.UTF8, token);
			_constants = await IniFile.ReadAsync(CreateFileLocation(true, Constants, _fixedDirectory), Encoding.UTF8, token);
			var newIni = await IniFile.ReadAsync(_iniFile, Encoding.UTF8, token);
			if (newIni != null)
			{
				_ini = newIni;
			}
			_initialReadDone = true;

			// Reset the sections that have already been registered
			FillSections();
		}

		/// <summary>
		/// Helper method to fill the values of one section
		/// </summary>
		/// <param name="section"></param>
		private void FillSection(IIniSection section)
		{
			string sectionName = section.GetSectionName();
			// Make sure there is no write protection
			section.RemoveWriteProtection();
			// Defaults:
			if (_defaults != null)
			{
				FillSection(_defaults, section);
			}
			// Ini:
			if (_ini != null)
			{
				FillSection(_ini, section);
			}
			// Constants:
			if (_constants != null)
			{
				section.StartWriteProtecting();
				FillSection(_constants, section);
				section.StopWriteProtecting();
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
			_ini = await IniFile.ReadAsync(stream, Encoding.UTF8, token);

			// Reset the current sections
			FillSections();
		}

		/// <summary>
		/// Internal method, use the supplied ini-sections & properties to fill the sectoins
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		private bool FillSections()
		{
			foreach (var section in _sections.Values)
			{
				FillSection(section);
			}
			return true;
		}

		/// <summary>
		/// Put the values from the iniProperties to the proxied object
		/// </summary>
		/// <param name="iniProperties"></param>
		/// <param name="iniSection"></param>
		private void FillSection(IDictionary<string, IDictionary<string, string>> iniSections, IIniSection iniSection)
		{

			IDictionary<string, string> iniProperties;
			iniSections.TryGetValue(iniSection.GetSectionName(), out iniProperties);

			IEnumerable<IniValue> iniValues = (from iniValue in iniSection.GetIniValues()
											   where iniValue.Behavior.Read
											   select iniValue);

			foreach (var iniValue in iniValues)
			{
				string dictionaryIdentifier = string.Format("{0}-{1}", iniSection.GetSectionName(), iniValue.IniPropertyName);
				// If there are no properties, there might still be a separate section for a dictionary
				if (iniValue.Converter != null && iniSections.ContainsKey(dictionaryIdentifier))
				{
					try
					{
						iniValue.Value = iniValue.Converter.ConvertFrom(iniSections[dictionaryIdentifier]);
					}
					catch
					{
						if (!iniValue.Behavior.IgnoreErrors)
						{
							throw;
						}
					}

					continue;
				}
				if (iniProperties == null)
				{
					continue;
				}
				string stringValue;
				// Skip values that don't have a property
				if (iniProperties.TryGetValue(iniValue.IniPropertyName, out stringValue))
				{
					Type stringType = typeof(string);
					Type destinationType = iniValue.ValueType;
					if (iniValue.Converter != null && iniValue.Converter.CanConvertFrom(stringType))
					{
						try
						{
							iniValue.Value = iniValue.Converter.ConvertFrom(stringValue);
						}
						catch
						{
							if (!iniValue.Behavior.IgnoreErrors)
							{
								throw;
							}
						}
						continue;
					}
					if (destinationType != stringType)
					{
						var converter = TypeDescriptor.GetConverter(destinationType);
						if (converter != null && converter.CanConvertFrom(stringType))
						{
							try
							{
								iniValue.Value = converter.ConvertFrom(stringValue);
							}
							catch
							{
								if (!iniValue.Behavior.IgnoreErrors)
								{
									throw;
								}
							}
							continue;
						}
					}
					// just set it and hope it can be cast
					iniValue.Value = stringValue;
				}
			}
		}
	}
}
