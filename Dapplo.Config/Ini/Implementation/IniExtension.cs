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

using System.Linq;
using System.Reflection;
using Dapplo.Config.Support;
using System.ComponentModel;
using Dapplo.Config.Extension.Implementation;

namespace Dapplo.Config.Ini.Implementation {
	/// <summary>
	/// Extend the PropertyProxy with Ini functionality
	/// </summary>
	[Extension(typeof(IIniSection))]
	internal class IniExtension<T> : AbstractPropertyProxyExtension<T> {

		public IniExtension(IPropertyProxy<T> proxy) : base(proxy) {
			CheckType(typeof(IIniSection));

			//_proxy.RegisterMethod(ConfigUtils.GetMemberName<IIniSection>(x => x.IniValueFor<T>(y => default(T))), IniValueFor);
			Proxy.RegisterMethod(ConfigUtils.GetMemberName<IIniSection, object>(x => x.GetIniValues()), GetIniValues);
			Proxy.RegisterMethod(ConfigUtils.GetMemberName<IIniSection, object>(x => x.GetSectionName()), GetSectionName);
			Proxy.RegisterMethod(ConfigUtils.GetMemberName<IIniSection, object>(x => x.GetSectionDescription()), GetDescription);
		}

		/// <summary>
		/// Supply the iniSection name
		/// </summary>
		private void GetSectionName(MethodCallInfo methodCallInfo) {
			var iniSectionAttribute = typeof(T).GetCustomAttribute<IniSectionAttribute>();
			if (iniSectionAttribute != null && !string.IsNullOrEmpty(iniSectionAttribute.Name)) {
				methodCallInfo.ReturnValue = iniSectionAttribute.Name;
			} else {
				methodCallInfo.ReturnValue = typeof(T).Name;
			}
		}

		/// <summary>
		/// Supply the GetSectionDescription
		/// </summary>
		private void GetDescription(MethodCallInfo methodCallInfo) {
			var descriptionAttribute = typeof(T).GetCustomAttribute<DescriptionAttribute>();
			if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description)) {
				methodCallInfo.ReturnValue = descriptionAttribute.Description;
			}
		}

		/// <summary>
		/// Get all the ini values, these are generated and not cached!
		/// </summary>
		private void GetIniValues(MethodCallInfo methodCallInfo) {
			// return a linq which loops over all the properties and generates GetIniValues
			methodCallInfo.ReturnValue = from propertyInfo in typeof(T).GetProperties()
										 select GenerateIniValue(propertyInfo);
		}

		/// <summary>
		/// Logic to generate all the ini value information
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IniValue</returns>
		private IniValue GenerateIniValue(PropertyInfo propertyInfo) {
			var newIniValue = new IniValue(Proxy.Properties);
			newIniValue.PropertyName = propertyInfo.Name;
			newIniValue.ValueType = propertyInfo.PropertyType;

			newIniValue.IniPropertyName = propertyInfo.GetDataMemberName();
			if (string.IsNullOrEmpty(newIniValue.IniPropertyName)) {
				newIniValue.IniPropertyName = newIniValue.PropertyName;
			}
			newIniValue.EmitDefaultValue = propertyInfo.GetEmitDefaultValue();
			newIniValue.Description = propertyInfo.GetDescription();
			newIniValue.Converter = propertyInfo.GetTypeConverter();
			newIniValue.DefaultValue = propertyInfo.GetDefaultValue();
			newIniValue.IsReadOnly = propertyInfo.GetReadOnly();
			newIniValue.Category = propertyInfo.GetCategory();

			return newIniValue;
		}
	}
}