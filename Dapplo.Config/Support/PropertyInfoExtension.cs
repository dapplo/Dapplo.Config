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

using Dapplo.Config.Ini;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;

namespace Dapplo.Config.Support {
	public static class PropertyInfoExtension {
		/// <summary>
		/// Create a default for the property.
		/// This can come from the DefaultValueFor from the DefaultValueAttribute
		/// Or it can be something like an empty collection
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>object with a default value</returns>
		public static object GetDefaultValue(this PropertyInfo propertyInfo) {
			var defaultValueAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
			if (defaultValueAttribute != null) {
				return defaultValueAttribute.Value;
			}
			if (propertyInfo.PropertyType.IsValueType) {
				// msdn information: If this PropertyInfo object is a value type and value is null, then the property will be set to the default value for that type.
				return null;
			}

			try {
				return propertyInfo.PropertyType.CreateInstance();
			}
				// ReSharper disable once EmptyGeneralCatchClause
			catch {
				// Ignore creating the default type, this might happen if there is no default constructor.
			}

			return null;
		}

		/// <summary>
		/// Retrieve the TypeConverter from the TypeConverterAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>TypeConverter</returns>
		public static TypeConverter GetTypeConverter(this PropertyInfo propertyInfo) {
			var typeConverterAttribute = propertyInfo.GetCustomAttribute<TypeConverterAttribute>();
			if (typeConverterAttribute != null && !string.IsNullOrEmpty(typeConverterAttribute.ConverterTypeName)) {
				Type typeConverterType = Type.GetType(typeConverterAttribute.ConverterTypeName);
				if (typeConverterType != null) {
					return (TypeConverter)Activator.CreateInstance(typeConverterType);
				}
			}
			return null;
		}

		/// <summary>
		/// Retrieve the Description from the DescriptionAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Description</returns>
		public static string GetDescription(this PropertyInfo propertyInfo) {
			var descriptionAttribute = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
			if (descriptionAttribute != null) {
				return descriptionAttribute.Description;
			}
			return null;
		}

		/// <summary>
		/// Retrieve the Name from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Name</returns>
		public static string GetDataMemberName(this PropertyInfo propertyInfo) {
			var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>();
			if (dataMemberAttribute != null) {
				if (!string.IsNullOrEmpty(dataMemberAttribute.Name)) {
					return dataMemberAttribute.Name;
				}
			}
			return null;
		}

		/// <summary>
		/// Retrieve the EmitDefaultValue from the DataMemberAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>EmitDefaultValue</returns>
		public static bool GetEmitDefaultValue(this PropertyInfo propertyInfo) {
			var dataMemberAttribute = propertyInfo.GetCustomAttribute<DataMemberAttribute>();
			if (dataMemberAttribute != null) {
				return dataMemberAttribute.EmitDefaultValue;
			}
			return false;
		}

		/// <summary>
		/// Check if the property is non serialized (annotated with the NonSerializedAttribute)
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>true if the NonSerialized attribute is set on the property</returns>
		public static IniPropertyBehaviorAttribute GetIniPropertyBehavior(this PropertyInfo propertyInfo) {
			var iniPropertyBehaviorAttribute = propertyInfo.GetCustomAttribute<IniPropertyBehaviorAttribute>();
			if (iniPropertyBehaviorAttribute == null) {
				iniPropertyBehaviorAttribute = new IniPropertyBehaviorAttribute();
			}
			return iniPropertyBehaviorAttribute;
		}

		/// <summary>
		/// Retrieve the IsReadOnly from the ReadOnlyAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IsReadOnly</returns>
		public static bool GetReadOnly(this PropertyInfo propertyInfo) {
			var readOnlyAttribute = propertyInfo.GetCustomAttribute<ReadOnlyAttribute>();
			if (readOnlyAttribute != null) {
				return readOnlyAttribute.IsReadOnly;
			}
			return false;
		}

		/// <summary>
		/// Retrieve the Category from the CategoryAttribute for the supplied PropertyInfo
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>Category</returns>
		public static string GetCategory(this PropertyInfo propertyInfo) {
			var categoryAttribute = propertyInfo.GetCustomAttribute<CategoryAttribute>();
			if (categoryAttribute != null) {
				return categoryAttribute.Category;
			}
			return null;
		}
	}
}
