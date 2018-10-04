//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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
using Microsoft.Win32;

#endregion

namespace Dapplo.Registry
{
	/// <summary>
	///     Attribute to lay a connection to the registry
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface)]
	public class RegistryAttribute : Attribute
	{
		private RegistryHive _hive = RegistryHive.CurrentUser;
		private RegistryView _view = RegistryView.Default;

		/// <summary>
		///     Default constructor
		/// </summary>
		public RegistryAttribute()
		{
			Kind = RegistryValueKind.Unknown;
			IgnoreErrors = true;
		}

		/// <summary>
		///     Constructor with path and value
		/// </summary>
		/// <param name="path">Path in the registry</param>
		/// <param name="valueName">Name of the value</param>
		public RegistryAttribute(string path, string valueName = null) : this()
		{
			Path = path;
			ValueName = valueName;
		}

		/// <summary>
		///     What hive to use, see RegistryHive
		/// </summary>
		public bool HasHive { get; private set; }

		/// <summary>
		///     Is there a view?
		/// </summary>
		public bool HasView { get; private set; }

		/// <summary>
		///     What hive to use, see RegistryHive
		/// </summary>
		public RegistryHive Hive
		{
			get => _hive;
			set
			{
				_hive = value;
				HasHive = true;
			}
		}

		/// <summary>
		///     Set ignore errors to false, if you want an exception when a parse error occurs.
		///     Default this is set to true, which will cause the property to have the "default" value.
		/// </summary>
		public bool IgnoreErrors { get; set; }

		/// <summary>
		///     Specify what kind of value
		/// </summary>
		public RegistryValueKind Kind { get; set; }

		/// <summary>
		///     Path to key
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		///     Value in key, can be null to select all values or "" to select the default value
		/// </summary>
		public string ValueName { get; set; }

		/// <summary>
		///     Ignore the path of the 
		/// </summary>
		public bool IgnoreBasePath { get; set; }

        /// <summary>
        ///     What View to use, see RegistryView
        /// </summary>
        public RegistryView View
		{
			get => _view;
	        set
			{
				_view = value;
				HasView = true;
			}
		}
	}
}