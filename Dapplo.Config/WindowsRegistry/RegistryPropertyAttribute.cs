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
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class RegistryPropertyAttribute : Attribute
	{
		private RegistryHive _hive = RegistryHive.CurrentUser;
		private RegistryView _view = RegistryView.Default;

		public RegistryPropertyAttribute()
		{
			Kind = RegistryValueKind.Unknown;
			IgnoreErrors = true;
		}

		public RegistryPropertyAttribute(string path, string value = null) : this()
		{
			Path = path;
			Value = value;
		}

		/// <summary>
		/// Specify what kind of value
		/// </summary>
		public RegistryValueKind Kind
		{
			get;
			set;
		}

		/// <summary>
		/// What hive to use, see RegistryHive
		/// </summary>
		public RegistryHive Hive
		{
			get
			{
				return _hive;
			}
			set
			{
				_hive = value;
				HasHive = true;
			}
		}

		/// <summary>
		/// What hive to use, see RegistryHive
		/// </summary>
		public bool HasHive
		{
			get;
			private set;
		}

		/// <summary>
		/// What View to use, see RegistryView
		/// </summary>
		public RegistryView View
		{
			get
			{
				return _view;
			}
			set
			{
				_view = value;
				HasView = true;
			}
		}

		/// <summary>
		/// Is there a view?
		/// </summary>
		public bool HasView
		{
			get;
			private set;
		}

		/// <summary>
		/// Path to key
		/// </summary>
		public string Path
		{
			get;
			set;
		}

		/// <summary>
		/// Value in key, can be null to select all values or "" to select the default value
		/// </summary>
		public string Value
		{
			get;
			set;
		}

		/// <summary>
		/// Set ignore errors to false, if you want an exception when a parse error occurs.
		/// Default this is set to true, which will cause the property to have the "default" value.
		/// </summary>
		public bool IgnoreErrors
		{
			get;
			set;
		}
	}
}