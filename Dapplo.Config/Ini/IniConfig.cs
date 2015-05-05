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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// The IniConfig is used to bind IIniSection proxy objects to an ini file.
	/// </summary>
	public class IniConfig
	{
		private readonly IDictionary<string, IIniSection> _sections = new Dictionary<string, IIniSection>();

		/// <summary>
		/// Add an ini section to this IniConfig
		/// </summary>
		/// <param name="section"></param>
		public void AddSection(IIniSection section)
		{
			var sectionName = section.GetSectionName();
			if (!_sections.ContainsKey(sectionName))
			{
				_sections.Add(sectionName, section);
			}
		}

		/// <summary>
		/// Reset all the values, in all the registered ini sections, to their default
		/// </summary>
		public void Reset()
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

		/// <summary>
		/// Write all the IIniSections to the ini file
		/// </summary>
		/// <param name="filename">File to write to</param>
		/// <returns>Task</returns>
		public async Task WriteToFile(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}

			// Create the file as a stream
			using (var stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
			{
				// Write the registered ini sections to the stream
				await WriteToStream(stream);
			}

		}

		/// <summary>
		/// Write all the IIniSections to the stream
		/// </summary>
		/// <param name="stream">Stream to write to</param>
		/// <returns>Task</returns>
		public async Task WriteToStream(Stream stream)
		{
			// Do not dispose the writer, this will close the supplied stream and that is not our job!
			var writer = new StreamWriter(stream, Encoding.UTF8);

			// Loop over the "registered" sections
			foreach (var section in _sections.Values)
			{
				// This flag tells us if the header for the section is already written
				bool isSectionHeaderWritten = false;

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
					if (!isSectionHeaderWritten) {
						await writer.WriteLineAsync();
						string description = section.GetSectionDescription();
						if (!string.IsNullOrEmpty(description)) {
							await writer.WriteLineAsync(string.Format(";{0}", description));
						}
						await writer.WriteLineAsync(string.Format("[{0}]", section.GetSectionName()));
						// Mark section header as written!
						isSectionHeaderWritten = true;
					}

					// Check if the property has a description, if so write it in the ini comment before the property
					if (!string.IsNullOrEmpty(iniValue.Description))
					{
						await writer.WriteLineAsync(string.Format(";{0}", iniValue.Description));
					}

					// Check if a converter is specified
					TypeConverter converter = iniValue.Converter;
					// If not, use the default converter for the property type
					if (converter == null)
					{
						converter = TypeDescriptor.GetConverter(iniValue.ValueType);
					}
					// Convert the value to a string
					var writingValue = converter.ConvertToInvariantString(iniValue.Value);
					// And write the value with the IniPropertyName (which does NOT have to be the property name) to the file
					await writer.WriteLineAsync(string.Format("{0}={1}", iniValue.IniPropertyName, writingValue));
				}
			}
			// Make sure the values are flushed, otherwise the information is not in the stream
			writer.Flush();
		}

		/// <summary>
		/// Initialize the IniConfig by reading all the properties from the file and setting them on the IniSections
		/// </summary>
		/// <returns>Task with bool indicating if the ini file was read</returns>
		public async Task<bool> ReadFromFile(string filename)
		{
			if (string.IsNullOrEmpty(filename))
			{
				throw new ArgumentNullException("filename");
			}
			if (File.Exists(filename))
			{
				var properties = await IniReader.ReadAsync(filename, Encoding.UTF8);
				return FillSections(properties);
			}
			return false;
		}

		/// <summary>
		/// Initialize the IniConfig by reading all the properties from the file and setting them on the IniSections
		/// </summary>
		/// <returns>Task with bool indicating if the ini file was read</returns>
		public async Task<bool> ReadFromStream(Stream stream)
		{
			var properties = await IniReader.ReadAsync(stream, Encoding.UTF8);
			return FillSections(properties);
		}

		/// <summary>
		/// Internal method, use the supplied ini-sections & properties to fill the sectoins
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		private bool FillSections(Dictionary<string, Dictionary<string, string>> properties)
		{
			foreach (var sectionName in properties.Keys)
			{
				IIniSection section;
				if (_sections.TryGetValue(sectionName, out section))
				{
					var iniProperties = properties[sectionName];
					FillSection(iniProperties, section);
				}
			}
			return true;
		}

		/// <summary>
		/// Put the values from the iniProperties to the proxied object
		/// </summary>
		/// <param name="iniProperties"></param>
		/// <param name="iniSection"></param>
		private void FillSection(IDictionary<string, string> iniProperties, IIniSection iniSection)
		{
			IDictionary<string, IniValue> iniValues = (from iniValue in iniSection.GetIniValues()
													   where iniValue.Behavior.Read
													   select iniValue).ToDictionary(x => x.IniPropertyName, x => x);
			foreach (var iniPropertyName in iniProperties.Keys)
			{
				IniValue iniValue;
				// Skip values that don't have a property
				if (iniValues.TryGetValue(iniPropertyName, out iniValue))
				{
					object stringValue = iniProperties[iniPropertyName];

					Type sourceType = typeof(string);
					Type destinationType = iniValue.ValueType;
					if (destinationType != sourceType || iniValue.Converter != null)
					{
						if (iniValue.Converter != null && iniValue.Converter.CanConvertFrom(sourceType))
						{
							iniValue.Value = iniValue.Converter.ConvertFrom(stringValue);
						}
						else
						{
							var converter = TypeDescriptor.GetConverter(destinationType);
							if (converter.CanConvertFrom(typeof(string)))
							{
								iniValue.Value = converter.ConvertFrom(stringValue);
							}
							else
							{
								// No converter, just set it and hope it can be cast
								iniValue.Value = stringValue;
							}
						}
					}
					else
					{
						iniValue.Value = stringValue;
					}
				}
			}
		}
	}
}
