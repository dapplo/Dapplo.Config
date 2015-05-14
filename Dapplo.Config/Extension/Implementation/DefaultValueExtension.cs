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
using Dapplo.Config.Support;
using System.Reflection;
using System.ComponentModel;

namespace Dapplo.Config.Extension.Implementation
{
	/// <summary>
	///  This implements logic to set the default values on your property interface.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof(IDefaultValue))]
	internal class DefaultValueExtension<T> : AbstractPropertyProxyExtension<T>
	{
		public DefaultValueExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof(IDefaultValue));

			// this registers one method and the overloading is handled in the GetDefaultValue
			Proxy.RegisterMethod(ConfigUtils.GetMemberName<IDefaultValue>(x => x.DefaultValueFor("")), GetDefaultValue);
			Proxy.RegisterMethod(ConfigUtils.GetMemberName<IDefaultValue>(x => x.RestoreToDefault("")), RestoreToDefault);
		}

		/// <summary>
		/// Process the property, in our case set the default
		/// </summary>
		/// <param name="propertyInfo"></param>
		public override void InitProperty(PropertyInfo propertyInfo)
		{
			var defaultValue = GetConvertedDefaultValue(propertyInfo);
			if (defaultValue != null)
			{
				Proxy.Properties[propertyInfo.Name] = defaultValue;
				return;
			}
			if (!propertyInfo.PropertyType.IsInterface && !propertyInfo.PropertyType.IsByRef && propertyInfo.PropertyType != typeof(string))
			{
				try {
					defaultValue = Activator.CreateInstance(propertyInfo.PropertyType);
					Proxy.Properties[propertyInfo.Name] = defaultValue;
					return;
				} catch {
				}
			}
			if (Proxy.Properties.ContainsKey(propertyInfo.Name)) {
				Proxy.Properties.Remove(propertyInfo.Name);
			}
		}

		/// <summary>
		/// Retrieve the default value, using the TypeConverter
		/// </summary>
		/// <param name="propertyInfo">Property to get the default value for</param>
		/// <returns>object with the type converted default value</returns>
		private object GetConvertedDefaultValue(PropertyInfo propertyInfo)
		{
			var defaultValue = propertyInfo.GetDefaultValue();
			if (defaultValue != null)
			{
				TypeConverter typeConverter = propertyInfo.GetTypeConverter();
				if (typeConverter != null && typeConverter.CanConvertFrom(defaultValue.GetType()))
				{
					// Convert
					return typeConverter.ConvertFrom(defaultValue);
				}
			}
			return defaultValue;
		}

		/// <summary>
		/// Return the default value for a property
		/// </summary>
		private void GetDefaultValue(MethodCallInfo methodCallInfo)
		{
			PropertyInfo propertyInfo = typeof(T).GetProperty(methodCallInfo.PropertyNameOf(0));
			methodCallInfo.ReturnValue = GetConvertedDefaultValue(propertyInfo);
		}

		/// <summary>
		/// Return the default value for a property
		/// </summary>
		private void RestoreToDefault(MethodCallInfo methodCallInfo)
		{
			PropertyInfo propertyInfo = typeof(T).GetProperty(methodCallInfo.PropertyNameOf(0));
			InitProperty(propertyInfo);
		}
	}
}