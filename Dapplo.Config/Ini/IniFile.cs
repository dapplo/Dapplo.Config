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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// Functionality to read/write a .ini file
	/// </summary>
	public static class IniFile
	{
		private const string SectionStart = "[";
		private const string SectionEnd = "]";
		private const string Comment = ";";

		private static readonly char[] Assignment =
		{
			'='
		};

		/// <summary>
		/// Read an ini file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>dictionary of sections - dictionaries with the properties</returns>
		public static async Task<IDictionary<string, IDictionary<string, string>>> ReadAsync(string path, Encoding encoding, CancellationToken token = default(CancellationToken))
		{
			if (File.Exists(path))
			{
				using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1024))
				{
					return await ReadAsync(fileStream, encoding, token).ConfigureAwait(false);
				}
			}
			return null;
		}

		/// <summary>
		/// Read an stream of an Ini-file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="stream">Stream e.g. fileStream with the ini content</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="token">CancellationToken</param>
		/// <returns>dictionary of sections - dictionaries with the properties</returns>
		public static async Task<IDictionary<string, IDictionary<string, string>>> ReadAsync(Stream stream, Encoding encoding, CancellationToken token = default(CancellationToken))
		{
			IDictionary<string, IDictionary<string, string>> ini = new Dictionary<string, IDictionary<string, string>>();

			// Do not dispose the reader, this will close the supplied stream and that is not our job!
			var reader = new StreamReader(stream, encoding);
			Dictionary<string, string> nameValues = new Dictionary<string, string>();
			while (!reader.EndOfStream && !token.IsCancellationRequested)
			{
				string line = await reader.ReadLineAsync().ConfigureAwait(false);
				if (line != null)
				{
					string cleanLine = line.Trim();
					if (cleanLine.Length == 0 || cleanLine.StartsWith(Comment))
					{
						continue;
					}
					if (cleanLine.StartsWith(SectionStart))
					{
						string section = line.Replace(SectionStart, "").Replace(SectionEnd, "").Trim();
						nameValues = new Dictionary<string, string>();
						ini.Add(section, nameValues);
					}
					else
					{
						string[] keyvalueSplitter = line.Split(Assignment, 2);
						string name = keyvalueSplitter[0];
						string inivalue = keyvalueSplitter.Length > 1 ? keyvalueSplitter[1] : null;
						inivalue = ReadEscape(inivalue);
						if (nameValues.ContainsKey(name))
						{
							nameValues[name] = inivalue;
						}
						else
						{
							nameValues.Add(name, inivalue);
						}
					}
				}
			}
			return ini;
		}

		/// <summary>
		/// change escaped newlines to newlines, and any other conversions that might be needed
		/// </summary>
		/// <param name="iniValue">encoded string value</param>
		/// <returns>string</returns>
		private static string ReadEscape(string iniValue)
		{
			if (!string.IsNullOrEmpty(iniValue))
			{
				iniValue = iniValue.Replace("\\n", "\n");
			}
			return iniValue;
		}

		/// <summary>
		/// Read an ini file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="sections">A dictionary with dictionaries with values for every section</param>
		/// <param name="sectionComments">A dictionary with the optional comments for the file</param>
		/// <param name="token">CancellationToken</param>
		public static async Task WriteAsync(string path, Encoding encoding, IDictionary<string, IDictionary<string, string>> sections, IDictionary<string, IDictionary<string, string>> sectionComments = null, CancellationToken token = default(CancellationToken))
		{
			using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024))
			{
				await WriteAsync(fileStream, encoding, sections, sectionComments, token).ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Write the supplied properties to the stream
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="encoding"></param>
		/// <param name="sections"></param>
		/// <param name="sectionsComments">Optional</param>
		/// <param name="token"></param>
		public static async Task WriteAsync(Stream stream, Encoding encoding, IDictionary<string, IDictionary<string, string>> sections, IDictionary<string, IDictionary<string, string>> sectionsComments = null, CancellationToken token = default(CancellationToken))
		{
			var writer = new StreamWriter(stream, Encoding.UTF8);

			Exception exception = null;
			try
			{
				foreach (var sectionKey in sections.Keys)
				{
					if (token.IsCancellationRequested)
					{
						break;
					}
					await writer.WriteLineAsync().ConfigureAwait(false);
					IDictionary<string, string> properties = sections[sectionKey];
					if (properties.Count == 0)
					{
						continue;
					}
					IDictionary<string, string> comments = null;
					if (sectionsComments != null)
					{
						sectionsComments.TryGetValue(sectionKey, out comments);
						string sectionDescription;
						// Section comment is stored with the sectionKey
						if (comments != null && comments.TryGetValue(sectionKey, out sectionDescription))
						{
							if (!string.IsNullOrEmpty(sectionDescription))
							{
								await writer.WriteLineAsync(string.Format(";{0}", sectionDescription)).ConfigureAwait(false);
							}
						}
					}
					await writer.WriteLineAsync(string.Format("[{0}]", sectionKey)).ConfigureAwait(false);
					foreach (var propertyName in properties.Keys)
					{
						if (token.IsCancellationRequested)
						{
							break;
						}
						string propertyComment;
						if (comments != null && comments.TryGetValue(propertyName, out propertyComment))
						{
							if (!string.IsNullOrEmpty(propertyComment))
							{
								await writer.WriteLineAsync(string.Format(";{0}", propertyComment)).ConfigureAwait(false);
							}
						}
						await writer.WriteLineAsync(string.Format("{0}={1}", propertyName, WriteEscape(properties[propertyName]))).ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				// Store Exception so it can be thrown later
				exception = ex;
			}
			// Make sure the values are flushed, otherwise the information is not in the stream
			await writer.FlushAsync().ConfigureAwait(false);

			// Throw the exception, if we caught one
			if (exception != null)
			{
				throw exception;
			}
		}

		/// <summary>
		/// change newlines to escaped newlines, and any other conversions that might be needed
		/// </summary>
		/// <param name="iniValue">string</param>
		/// <returns>encoded string value</returns>
		private static string WriteEscape(string iniValue)
		{
			if (!string.IsNullOrEmpty(iniValue))
			{
				iniValue = iniValue.Replace("\n", "\\n");
			}
			return iniValue;
		}
	}
}