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
using System.Text.RegularExpressions;

namespace Dapplo.Config.Support
{
	public static class FileScanner
	{
		/// <summary>
		/// Scan the supplied directories for files which match the passed file pattern
		/// </summary>
		/// <param name="directories"></param>
		/// <param name="filePattern"></param>
		/// <returns>IEnumerable&lt;Tuple&lt;string,Match&gt;&gt;</returns>
		public static IEnumerable<Tuple<string, Match>> Scan(ICollection<string> directories, Regex filePattern)
		{
			var files = from path in directories
						where Directory.Exists(path)
						from file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
						select file;

			return from filePath in files
				   let match = Tuple.Create<string, Match>(filePath, filePattern.Match(filePath))
				   where match.Item2.Success
				   select match;
		}
	}
}
