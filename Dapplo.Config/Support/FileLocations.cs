/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Config

	Dapplo.Config is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Config is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have Config a copy of the GNU Lesser General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.
 */

using Dapplo.LogFacade;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// Some utils for managing the location of files
	/// </summary>
	public static class FileLocations
	{
		private static readonly LogSource Log = new LogSource();
		/// <summary>
		/// Scan the supplied directories for files which match the passed file pattern
		/// </summary>
		/// <param name="directories"></param>
		/// <param name="filePattern">Regular expression for the filename</param>
		/// <returns>IEnumerable&lt;Tuple&lt;string,Match&gt;&gt;</returns>
		public static IEnumerable<Tuple<string, Match>> Scan(ICollection<string> directories, Regex filePattern)
		{
			return from path in directories
				where Directory.Exists(path)
				from file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
				let match = filePattern.Match(file)
				where match.Success
				select Tuple.Create(file, match);
		}

		/// <summary>
		/// Scan the supplied directories for files which match the passed file pattern
		/// </summary>
		/// <param name="directories"></param>
		/// <param name="simplePattern"></param>
		/// <returns>IEnumerable&lt;string&gt;</returns>
		public static IEnumerable<string> Scan(ICollection<string> directories, string simplePattern)
		{
			return from path in directories
				   where Directory.Exists(path)
				   from file in Directory.EnumerateFiles(path, simplePattern, SearchOption.AllDirectories)
				   select file;
		}

		/// <summary>
		/// Get the startup location, which is either the location of the entry assemby, or the executing assembly
		/// </summary>
		/// <returns>string with the directory of where the running code/applicationName was started</returns>
		public static string StartupDirectory
		{
			get {
				var directory = AppDomain.CurrentDomain.BaseDirectory;
				Log.Debug().WriteLine("StartupDirectory {0}", directory);
				return directory;
			}
		}

		/// <summary>
		/// Get the roaming AppData directory 
		/// </summary>
		/// <returns>string with the directory the appdata roaming directory</returns>
		public static string RoamingAppDataDirectory(string applicationName)
		{
			var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);
			Log.Debug().WriteLine("RoamingAppDataDirectory {0}", directory);
			return directory;
		}
	}
}
