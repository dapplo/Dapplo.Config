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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// Helps to specify how the ini file is read
	/// </summary>
	public enum ConfigFilePolicy
	{
		Portable,
		Basic,
		Fixed
	}

	/// <summary>
	/// This manages the location of the Ini-File for one instance
	/// </summary>
	public class ConfigFilenameManager
	{
		private readonly string _filename;
		private readonly ConfigFilePolicy _policy;

		/// <summary>
		/// Setup the management of an .ini file location
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="policy"></param>
		public ConfigFilenameManager(string fileName, ConfigFilePolicy policy = ConfigFilePolicy.Basic)
		{
			_filename = fileName;
			_policy = policy;
		}

		public string WritableIniLocation
		{
			get
			{
				return _filename;
			}
		}

		public string ConstantLocation
		{
			get
			{
				return _filename;
			}
		}
	}
}
