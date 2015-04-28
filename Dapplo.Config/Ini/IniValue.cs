﻿/*
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

using Dapplo.Config.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dapplo.Config.Ini {
	/// <summary>
	/// Container for supplying the properties to the Ini file
	/// </summary>
	public class IniValue {
		private IDictionary<string, object> _properties;
		public IniValue(IDictionary<string, object> properties) {
			_properties = properties;
		}

		/// <summary>
		/// Name of the property in the interface
		/// </summary>
		public string PropertyName {
			get;
			set;
		}

		/// <summary>
		/// Name of the property in the file, this could be different
		/// </summary>
		public string IniPropertyName {
			get;
			set;
		}

		/// <summary>
		/// Description, which was set via the DescriptionAttribute on the property
		/// </summary>
		public string Description {
			get;
			set;
		}

		/// <summary>
		/// Category, which was set via the CategoryAttribute on the property
		/// </summary>
		public string Category {
			get;
			set;
		}

		/// <summary>
		/// Current value
		/// </summary>
		public object Value {
			get {
				return _properties.SafeGet(PropertyName);
			}
			set {
				_properties.SafelyAddOrOverwrite(PropertyName, value);
			}
		}

		/// <summary>
		/// Default value, from the DefaultAttribute
		/// </summary>
		public object DefaultValue {
			get;
			set;
		}

		/// <summary>
		/// If this is set to true, we also should write the default value to the file
		/// </summary>
		public bool EmitDefaultValue {
			get;
			set;
		}

		/// <summary>
		/// Type for the value, needed for conversion when reading.
		/// </summary>
		public Type ValueType {
			get;
			set;
		}

		/// <summary>
		/// Return the TypeConverter for this value, when not set defaults are used
		/// </summary>
		public TypeConverter Converter {
			get;
			set;
		}

		/// <summary>
		/// Return true when the value is readonly in the GUI
		/// </summary>
		public bool IsReadOnly {
			get;
			set;
		}

		/// <summary>
		/// Check if this IniValue has a value
		/// </summary>
		public bool HasValue {
			get {
				return _properties.ContainsKey(PropertyName);
			}
		}

		/// <summary>
		/// Check if this IniValue needs to be written.
		/// This returns false if there is no value, or if the value is the default and if EmitDefaultValue is false (Default)
		/// </summary>
		public bool IsWriteNeeded {
			get {
				// if EmitDefaultValue is true, we should always write this value (without checking if it is default
				if (EmitDefaultValue) {
					return true;
				}

				// Don't write if there is no value
				if (!_properties.ContainsKey(PropertyName)) {
					return false;
				}

				object value = _properties.SafeGet(PropertyName);

				// Check if our value is default
				bool isDefault = object.Equals(value, DefaultValue);
				return !isDefault;
			}
		}
	}
}
