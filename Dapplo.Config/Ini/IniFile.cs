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
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Log;

#endregion

namespace Dapplo.Config.Ini
{
	/// <summary>
	///     Functionality to read/write a .ini file
	/// </summary>
	public static class IniFile
	{
		private const string SectionStart = "[";
		private const string SectionEnd = "]";
		private const string Comment = ";";
		private static readonly LogSource Log = new LogSource();

		private static readonly char[] Assignment =
		{
			'='
		};

		/// <summary>
		///     Read an ini file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>dictionary of sections - dictionaries with the properties</returns>
		public static async Task<IDictionary<string, IDictionary<string, string>>> ReadAsync(string path, Encoding encoding,
			CancellationToken cancellationToken  = default(CancellationToken))
		{
			if (!File.Exists(path))
			{
				Log.Verbose().WriteLine("Ini file {0} not found.", path);
				return null;
			}
			Log.Verbose().WriteLine("Reading ini file from {0}", path);
			using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024))
			{
				return await ReadAsync(fileStream, encoding, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		///     Read an stream of an Ini-file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and
		///     values.
		/// </summary>
		/// <param name="stream">Stream e.g. fileStream with the ini content</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>dictionary of sections - dictionaries with the properties</returns>
		public static async Task<IDictionary<string, IDictionary<string, string>>> ReadAsync(Stream stream, Encoding encoding, CancellationToken cancellationToken  = default(CancellationToken))
		{
			IDictionary<string, IDictionary<string, string>> ini = new Dictionary<string, IDictionary<string, string>>();

			// Do not dispose the reader, this will close the supplied stream and that is not our job!
			var reader = new StreamReader(stream, encoding);
			var nameValues = new Dictionary<string, string>();
			while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
			{
				var line = await reader.ReadLineAsync().ConfigureAwait(false);
				if (line == null)
				{
					continue;
				}
				var cleanLine = line.Trim();
				if (cleanLine.Length == 0 || cleanLine.StartsWith(Comment))
				{
					continue;
				}
				if (cleanLine.StartsWith(SectionStart))
				{
					var section = line.Replace(SectionStart, string.Empty).Replace(SectionEnd, string.Empty).Trim();
					nameValues = new Dictionary<string, string>();
					ini.Add(section, nameValues);
				}
				else
				{
					var keyvalueSplitter = line.Split(Assignment, 2);
					var name = keyvalueSplitter[0];
					var inivalue = keyvalueSplitter.Length > 1 ? keyvalueSplitter[1] : null;
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
			return ini;
		}

		/// <summary>
		///     change escaped newlines to newlines, and any other conversions that might be needed
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
		///     Read an ini file to a Dictionary, each key is a iniSection and the value is a Dictionary with name and values.
		/// </summary>
		/// <param name="path">Path to file</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="sections">A dictionary with dictionaries with values for every section</param>
		/// <param name="sectionComments">A dictionary with the optional comments for the file</param>
		/// <param name="cancellationToken">CancellationToken</param>
		public static async Task WriteAsync(string path, Encoding encoding, IDictionary<string, IDictionary<string, string>> sections,
			IDictionary<string, IDictionary<string, string>> sectionComments = null, CancellationToken cancellationToken  = default(CancellationToken))
		{
			Log.Verbose().WriteLine("Writing ini values to {0}", path);
			using (var fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write, 1024))
			{
				await WriteAsync(fileStream, encoding, sections, sectionComments, cancellationToken).ConfigureAwait(false);
			}
		}

		/// <summary>
		///     Write the supplied properties to the stream
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="encoding">Encoding</param>
		/// <param name="sections">IDictionary</param>
		/// <param name="sectionsComments">Optional IDictionary for comments</param>
		/// <param name="cancellationToken">CancellationToken</param>
		public static async Task WriteAsync(Stream stream, Encoding encoding, IDictionary<string, IDictionary<string, string>> sections,
			IDictionary<string, IDictionary<string, string>> sectionsComments = null, CancellationToken cancellationToken  = default(CancellationToken))
		{
			var isFirstLine = true;
			var writer = new StreamWriter(stream, encoding);

			Exception exception = null;
			try
			{
				foreach (var sectionKey in sections.Keys)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						break;
					}

					var properties = sections[sectionKey];
					if (properties.Count == 0)
					{
						continue;
					}
					if (!isFirstLine)
					{
						await writer.WriteLineAsync().ConfigureAwait(false);
					}
					isFirstLine = false;

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
								await writer.WriteLineAsync($";{sectionDescription}").ConfigureAwait(false);
							}
						}
					}
					await writer.WriteLineAsync($"[{sectionKey}]").ConfigureAwait(false);
					foreach (var propertyName in properties.Keys)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							break;
						}
						string propertyComment;
						if (comments != null && comments.TryGetValue(propertyName, out propertyComment))
						{
							if (!string.IsNullOrEmpty(propertyComment))
							{
								await writer.WriteLineAsync($";{propertyComment}").ConfigureAwait(false);
							}
						}
						await writer.WriteLineAsync($"{propertyName}={WriteEscape(properties[propertyName])}").ConfigureAwait(false);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine(ex.Message);
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
		///     change newlines to escaped newlines, and any other conversions that might be needed
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