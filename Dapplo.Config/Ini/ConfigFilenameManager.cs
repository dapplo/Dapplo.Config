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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// This manages the location of the Ini-File for one instance
	/// </summary>
	public class ConfigFilenameManager
	{
		private readonly string _fileName;
		private readonly string _applicationName;
		private const string Defaults = "-defaults";
		private const string Constants = "-constants";
		private const string IniExtension = "ini";

		/// <summary>
		/// Setup the management of an .ini file location
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="policy"></param>
		public ConfigFilenameManager(string applicationName, string fileName)
		{
			_applicationName = applicationName;
			_fileName = fileName;
		}

		/// <summary>
		/// Return the location of the ini file
		/// </summary>
		/// <param name="fixedDirectory"></param>
		/// <returns>string with full path to the ini file</returns>
		public string IniFileLocation(string fixedDirectory = null) {
			string iniFile;
			if (fixedDirectory != null)
			{
				iniFile = Path.Combine(fixedDirectory, string.Format("{0}.{1}", _fileName, IniExtension));
			}
			else
			{
				iniFile = Path.Combine(AppDataDirectory, string.Format("{0}.{1}", _fileName, IniExtension));
			}
			return iniFile;
		}

		/// <summary>
		/// Create the reading list for the files
		/// </summary>
		/// <param name="fixedDirectory">If you want to read the file(s) from a certain directory specify it here</param>
		/// <returns>reading list</returns>
		public string[] FileReadOrder(string fixedDirectory = null) {
			string defaultsFile;
			string constantsFile;
			string iniFile = IniFileLocation(fixedDirectory);

			if (fixedDirectory != null)
			{
				defaultsFile = Path.Combine(fixedDirectory, string.Format("{0}{1}.{2}", _fileName, Defaults, IniExtension));
				constantsFile = Path.Combine(fixedDirectory, string.Format("{0}{1}.{2}", _fileName, Constants, IniExtension));
			}
			else
			{
				defaultsFile = Path.Combine(StartupDirectory, string.Format("{0}{1}.{2}", _fileName, Defaults, IniExtension));
				if (!File.Exists(defaultsFile))
				{
					defaultsFile = Path.Combine(AppDataDirectory, string.Format("{0}{1}.{2}", _fileName, Defaults, IniExtension));
				}
				constantsFile = Path.Combine(StartupDirectory, string.Format("{0}{1}.{2}", _fileName, Constants, IniExtension));
				if (!File.Exists(constantsFile))
				{
					constantsFile = Path.Combine(AppDataDirectory, string.Format("{0}{1}.{2}", _fileName, Constants, IniExtension));
				}
			}
			return new string[]{defaultsFile, iniFile, constantsFile};
		}

		/// <summary>
		/// Load the ini files "default", "normal" and fixed
		/// </summary>
		/// <param name="config">IniConfig to load</param>
		/// <param name="fixedDirectory">If you want to read the file(s) from a certain directory specify it here</param>
		public async Task LoadAsync(IniConfig config, string fixedDirectory = null) {
			foreach(string filename in FileReadOrder(fixedDirectory))
			{
				if (filename == null || !File.Exists(filename))
				{
					continue;
				}
				await config.ReadFromFileAsync(filename);
			}
		}

		/// <summary>
		/// Write the ini file
		/// </summary>
		/// <param name="config">IniConfig to load</param>
		/// <param name="fixedDirectory">If you want to read the file(s) from a certain directory specify it here</param>
		public async Task WriteAsync(IniConfig config, string fixedDirectory = null) {
			await config.WriteToFileAsync(IniFileLocation(fixedDirectory));
		}

		/// <summary>
		/// Retrieve the startup directory
		/// </summary>
		private string StartupDirectory {
			get {
				return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			}
		}

		/// <summary>
		/// Retrieve the ApplicationData directory
		/// </summary>
		private string AppDataDirectory {
			get {
				return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), _applicationName);
			}
		}
	}
}
