/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
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

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// The supported commands for the ini-REST api
	/// </summary>
	public enum IniRestCommands
	{
		Set,
		Get,
		Reset,
		Add,
		Remove
	}

	/// <summary>
	/// This is a container for the IniRest, it has all the needed information to process
	/// </summary>
	public class IniRestCommand
	{
		/// <summary>
		/// The command to process
		/// </summary>
		public IniRestCommands Command
		{
			get;
			set;
		}

		/// <summary>
		/// The application for this rest command
		/// </summary>
		public string Application
		{
			get;
			set;
		}

		/// <summary>
		/// The ini file for this rest command
		/// </summary>
		public string File
		{
			get;
			set;
		}

		/// <summary>
		/// The ini-section for this rest command
		/// </summary>
		public string Section
		{
			get;
			set;
		}

		/// <summary>
		/// For add / remove we NEED a target, for set/reset it's possible to specify one
		/// </summary>
		public string Target
		{
			get;
			set;
		}

		/// <summary>
		/// These values are only keys when get, reset or remove, key/values when set or add
		/// </summary>
		public IDictionary<string, string> Values
		{
			get;
			set;
		} = new Dictionary<string, string>();

		/// <summary>
		/// The IniValues that were specified for the get/set/reset or add/remove
		/// </summary>
		public IList<IniValue> Results
		{
			get;
			set;
		} = new List<IniValue>();
	}
}
