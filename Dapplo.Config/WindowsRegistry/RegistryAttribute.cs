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
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using Dapplo.Config.WindowsRegistry.Implementation;
using Microsoft.Win32;
using Dapplo.InterfaceImpl;

#endregion

namespace Dapplo.Config.WindowsRegistry
{
	/// <summary>
	///     Specify the base settings for the registry property proxy interface
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
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
		///     What hive to use, see RegistryHive
		/// </summary>
		public RegistryHive Hive { get; set; }

		/// <summary>
		///     Path to key
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		///     What view to use, default is Default
		/// </summary>
		public RegistryView View { get; set; }
	}
}