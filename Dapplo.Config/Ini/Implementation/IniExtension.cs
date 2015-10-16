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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dapplo.Config.Ini.Implementation
{
	/// <summary>
	/// Extend the PropertyProxy with Ini functionality
	/// </summary>
	[Extension(typeof (IIniSection))]
	internal class IniExtension<T> : AbstractPropertyProxyExtension<T>
	{
		private IReadOnlyDictionary<string, IniValue> _iniValues;
		private readonly IniSectionAttribute _iniSectionAttribute = typeof(T).GetCustomAttribute<IniSectionAttribute>();

		public IniExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (IIniSection));

			//_proxy.RegisterMethod(ConfigUtils.GetMemberName<IIniSection>(x => x.IniValueFor<T>(y => default(T))), IniValueFor);
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IIniSection, object>(x => x.GetIniValues()), GetIniValues);
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IIniSection, object>(x => x.GetIniValue(null)), GetIniValue);
            Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IIniSection, object>(x => x[null]), GetIniValue);

            IniValue dummy;
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IIniSection, object>(x => x.TryGetIniValue(null, out dummy)), TryGetIniValue);
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IIniSection, object>(x => x.GetSectionName()), GetSectionName);
			Proxy.RegisterMethod(ExpressionExtensions.GetMemberName<IIniSection, object>(x => x.GetSectionDescription()), GetDescription);
		}

		/// <summary>
		/// Cache the inivalues, remove errors that can be ignored
		/// </summary>
		public override void AfterInitialization()
		{
			base.AfterInitialization();

			var iniValues = new NonStrictLookup<IniValue>();
			_iniValues = new ReadOnlyDictionary<string, IniValue>(iniValues);

			var generatedIniValues = from propertyInfo in Proxy.AllPropertyInfos.Values
									 select GenerateIniValue(propertyInfo);

			foreach (var iniValue in generatedIniValues)
			{
				iniValues.Add(iniValue.PropertyName, iniValue);
			}

			foreach (var propertyName in Proxy.InitializationErrors.Keys.ToList())
			{
				IniValue currentValue;
				if (_iniValues.TryGetValue(propertyName, out currentValue))
				{
					if (currentValue.Behavior.IgnoreErrors)
					{
						Proxy.InitializationErrors.Remove(propertyName);
						continue;
					}
					throw Proxy.InitializationErrors[propertyName];
				}
			}
		}


		/// <summary>
		/// Get all the ini values, these are generated and not cached!
		/// </summary>
		private void GetIniValues(MethodCallInfo methodCallInfo) {
			// return a linq which loops over all the properties and generates GetIniValues
			// This prevents that the collection can be modified
			methodCallInfo.ReturnValue = _iniValues;
		}

		/// <summary>
		/// Get a single ini value
		/// </summary>
		private void GetIniValue(MethodCallInfo methodCallInfo) {
			IniValue outIniValue;
			if (_iniValues.TryGetValue(methodCallInfo.PropertyNameOf(0), out outIniValue))
			{
				// return IniValue
				methodCallInfo.ReturnValue = outIniValue;
			}
		}

		/// <summary>
		/// Try to get a single ini value
		/// </summary>
		private void TryGetIniValue(MethodCallInfo methodCallInfo) {
			IniValue outIniValue;
			methodCallInfo.ReturnValue = _iniValues.TryGetValue(methodCallInfo.PropertyNameOf(0), out outIniValue);
			methodCallInfo.OutArguments = new object[] { outIniValue };
		}

		/// <summary>
		/// Supply the iniSection name
		/// </summary>
		private void GetSectionName(MethodCallInfo methodCallInfo)
		{
			if (_iniSectionAttribute != null && !string.IsNullOrEmpty(_iniSectionAttribute.Name))
			{
				methodCallInfo.ReturnValue = _iniSectionAttribute.Name;
			}
			else
			{
				methodCallInfo.ReturnValue = typeof (T).Name;
			}
		}

		/// <summary>
		/// Supply the GetSectionDescription
		/// </summary>
		private void GetDescription(MethodCallInfo methodCallInfo)
		{
			var descriptionAttribute = typeof (T).GetCustomAttribute<DescriptionAttribute>();
			if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description))
			{
				methodCallInfo.ReturnValue = descriptionAttribute.Description;
			}
		}

		/// <summary>
		/// Logic to generate all the ini value information
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IniValue</returns>
		private IniValue GenerateIniValue(PropertyInfo propertyInfo)
		{
			var newIniValue = new IniValue(Proxy);
			newIniValue.PropertyName = propertyInfo.Name;
			newIniValue.ValueType = propertyInfo.PropertyType;
			newIniValue.IniPropertyName = propertyInfo.GetDataMemberName();
			if (string.IsNullOrEmpty(newIniValue.IniPropertyName))
			{
				newIniValue.IniPropertyName = newIniValue.PropertyName;
			}
			newIniValue.EmitDefaultValue = propertyInfo.GetEmitDefaultValue();
			newIniValue.Description = propertyInfo.GetDescription();
			newIniValue.Converter = propertyInfo.GetTypeConverter();
			newIniValue.Category = propertyInfo.GetCategory();
			newIniValue.Behavior = propertyInfo.GetIniPropertyBehavior();
			if (!newIniValue.Behavior.IsIgnoreErrorsSet)
			{
				newIniValue.Behavior.IgnoreErrors = _iniSectionAttribute.IgnoreErrors;
            }
			newIniValue.DefaultValue = propertyInfo.GetDefaultValue();
			return newIniValue;
		}
	}
}