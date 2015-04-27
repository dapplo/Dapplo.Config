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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini {
	/// <summary>
	/// Functionality to read a .ini file
	/// </summary>
	internal static class IniReader {
		private const string SECTION_START = "[";
		private const string SECTION_END = "]";
		private const string COMMENT = ";";
		private static readonly char[] ASSIGNMENT = new char[] { '=' };

		/// <summary>
		/// Read an ini file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>dictionary of sections - dictionaries with the properties</returns>
		public static async Task<Dictionary<string, Dictionary<string, string>>> ReadAsync(string path, Encoding encoding, CancellationToken token = default(CancellationToken)) {
			using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024)) {
				return await ReadAsync(fileStream, encoding, token);
			}
		}

		/// <summary>
		/// Read an stream of an Ini-file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="stream">Stream e.g. fileStream with the ini content</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>dictionary of sections - dictionaries with the properties</returns>
		public static async Task<Dictionary<string, Dictionary<string, string>>> ReadAsync(Stream stream, Encoding encoding, CancellationToken token = default(CancellationToken)) {
			Dictionary<string, Dictionary<string, string>> ini = new Dictionary<string, Dictionary<string, string>>();
			using (StreamReader reader = new StreamReader(stream, encoding)) {
				Dictionary<string, string> nameValues = new Dictionary<string, string>();
				while (!reader.EndOfStream && !token.IsCancellationRequested) {
					string line = await reader.ReadLineAsync();
					if (line != null) {
						string cleanLine = line.Trim();
						if (cleanLine.Length == 0 || cleanLine.StartsWith(COMMENT)) {
							continue;
						}
						if (cleanLine.StartsWith(SECTION_START)) {
							string section = line.Replace(SECTION_START, "").Replace(SECTION_END, "").Trim();
							nameValues = new Dictionary<string, string>();
							ini.Add(section, nameValues);
						} else {
							string[] keyvalueSplitter = line.Split(ASSIGNMENT, 2);
							string name = keyvalueSplitter[0];
							string inivalue = keyvalueSplitter.Length > 1 ? keyvalueSplitter[1] : null;
							inivalue = ConvertSpecialCharacters(inivalue);
							if (nameValues.ContainsKey(name)) {
								nameValues[name] = inivalue;
							} else {
								nameValues.Add(name, inivalue);
							}
						}
					}
				}
			}
			return ini;
		}

		/// <summary>
		/// change escaped newlines to newlines, an any other conversions that might be needed
		/// </summary>
		/// <param name="iniValue">string</param>
		/// <returns>string</returns>
		private static string ConvertSpecialCharacters(string iniValue) {
			iniValue = iniValue.Replace("\\n", "\n");
			return iniValue;
		}
	}
}
