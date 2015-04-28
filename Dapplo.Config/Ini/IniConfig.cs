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

namespace Dapplo.Config.Ini {
	/// <summary>
	/// The IniConfig is used to bind IIniSection proxy objects to an ini file.
	/// </summary>
	public class IniConfig {
		private readonly string _filename;
		private readonly IDictionary<string, IIniSection> _sections = new Dictionary<string, IIniSection>();

		/// <summary>
		/// Create a binding between the specified files and the IIniSection proxy objects in this instance
		/// </summary>
		/// <param name="filename">File to read from and write to</param>
		public IniConfig(string filename) {
			_filename = filename;
		}

		/// <summary>
		/// Add an ini section to this IniConfig
		/// </summary>
		/// <param name="section"></param>
		public void AddSection(IIniSection section) {
			if (!_sections.ContainsKey(section.SectionName)) {
				_sections.Add(section.SectionName, section);
			}
		}

		/// <summary>
		/// Write all the IIniSections to the ini file
		/// </summary>
		public async Task Write() {
			using (var stream = new FileStream(_filename, FileMode.Create, FileAccess.Write))
			using (var writer = new StreamWriter(stream, Encoding.UTF8)) {
				foreach(var section in _sections.Values) {
					await writer.WriteLineAsync();
					string description = section.Description;
					if (!string.IsNullOrEmpty(description)) {
						await writer.WriteLineAsync(string.Format(";{0}", description));
					}
					await writer.WriteLineAsync(string.Format("[{0}]", section.SectionName));
					foreach (var iniValue in section.IniValues) {
						if (!iniValue.IsWriteNeeded) {
							continue;
						}
						if (!string.IsNullOrEmpty(iniValue.Description)) {
							await writer.WriteLineAsync(string.Format(";{0}", iniValue.Description));
						}
						TypeConverter converter = iniValue.Converter;
						if (converter == null) {
							converter = TypeDescriptor.GetConverter(iniValue.ValueType);
						}
						await writer.WriteLineAsync(string.Format("{0}={1}", iniValue.IniPropertyName, converter.ConvertToInvariantString(iniValue.Value)));
					}
				}
			}
		}

		/// <summary>
		/// Initialize the IniConfig by reading all the properties from the file and setting them on the IniSections
		/// </summary>
		/// <returns>Task with bool indicating if the ini file was read</returns>
		public async Task<bool> Init() {
			if (File.Exists(_filename)) {
				var properties = await IniReader.ReadAsync(_filename, Encoding.UTF8);
				foreach (var sectionName in properties.Keys) {
					IIniSection section;
					if (_sections.TryGetValue(sectionName, out section)) {
						var iniProperties = properties[sectionName];
						FillSection(iniProperties, section);
					}
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Put the values from the iniProperties to the proxied object
		/// </summary>
		/// <param name="iniProperties"></param>
		/// <param name="iniSection"></param>
		private void FillSection(IDictionary<string, string> iniProperties, IIniSection iniSection) {
			IDictionary<string, IniValue> iniValues = (from iniValue in iniSection.IniValues
													   select iniValue).ToDictionary(x => x.IniPropertyName, x => x);
			foreach (var iniPropertyName in iniProperties.Keys) {
				IniValue iniValue;
				// Skip values that don't have a property
				if (iniValues.TryGetValue(iniPropertyName, out iniValue)) {
					object stringValue = iniProperties[iniPropertyName];

					Type sourceType = typeof(string);
					Type destinationType = iniValue.ValueType;
					if (destinationType != sourceType) {
						var converter = TypeDescriptor.GetConverter(destinationType);
						if (converter.CanConvertFrom(typeof(string))) {
							iniValue.Value = converter.ConvertFrom(stringValue);
						} else if (iniValue.Converter != null && iniValue.Converter.CanConvertFrom(sourceType)) {
							iniValue.Value = iniValue.Converter.ConvertFrom(stringValue);
						} else {
							// No converter, just set it and hope it can be cast
							iniValue.Value = stringValue;
						}
					} else {
						iniValue.Value = stringValue;
					}
				}
			}
		}
	}
}
