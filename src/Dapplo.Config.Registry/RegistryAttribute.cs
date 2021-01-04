// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Win32;

namespace Dapplo.Config.Registry
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