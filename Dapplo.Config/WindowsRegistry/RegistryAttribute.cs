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

using Microsoft.Win32;
using System;

namespace Dapplo.Config.WindowsRegistry
{
	/// <summary>
	/// Specify the base settings for the registry property proxy interface
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false)]
	public class RegistryAttribute : Attribute
	{
		public RegistryAttribute()
		{
			View = RegistryView.Default;
			Hive = RegistryHive.CurrentUser;
		}

		public RegistryAttribute(string path) : this()
		{
			Path = path;
		}

		/// <summary>
		/// What hive to use, see RegistryHive
		/// </summary>
		public RegistryHive Hive
		{
			get;
			set;
		}

		/// <summary>
		/// What view to use, default is Default
		/// </summary>
		public RegistryView View
		{
			get;
			set;
		}

		/// <summary>
		/// Path to key
		/// </summary>
		public string Path
		{
			get;
			set;
		}
	}
}
