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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Dapplo.Config.Interceptor;
using Dapplo.Config.Proxy.Implementation;
using Dapplo.Config.Support;

#endregion

namespace Dapplo.Config.Ini.Implementation
{
	/// <summary>
	///     Extend the PropertyProxy with Ini functionality
	/// </summary>
	[Extension(typeof (IIniSection))]
	internal class IniExtension<T> : AbstractPropertyProxyExtension<T>
	{
		private readonly IniSectionAttribute _iniSectionAttribute = typeof (T).GetCustomAttribute<IniSectionAttribute>();
		private IReadOnlyDictionary<string, IniValue> _iniValues;

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
		///     Cache the inivalues, remove errors that can be ignored
		/// </summary>
		public override void AfterInitialization()
		{
			base.AfterInitialization();

			var iniValues = new Dictionary<string, IniValue>(AbcComparer.Instance);
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
		///     Logic to generate all the ini value information
		/// </summary>
		/// <param name="propertyInfo">PropertyInfo</param>
		/// <returns>IniValue</returns>
		private IniValue GenerateIniValue(PropertyInfo propertyInfo)
		{
			var newIniValue = new IniValue(Proxy)
			{
				PropertyName = propertyInfo.Name,
				ValueType = propertyInfo.PropertyType,
				IniPropertyName = propertyInfo.GetDataMemberName() ?? propertyInfo.Name,
				EmitDefaultValue = propertyInfo.GetEmitDefaultValue(),
				Description = propertyInfo.GetDescription(),
				Converter = propertyInfo.GetTypeConverter(),
				Category = propertyInfo.GetCategory(),
				Behavior = propertyInfo.GetIniPropertyBehavior(),
				DefaultValue = propertyInfo.GetDefaultValue()
			};
			if (!newIniValue.Behavior.IsIgnoreErrorsSet && _iniSectionAttribute != null)
			{
				newIniValue.Behavior.IgnoreErrors = _iniSectionAttribute.IgnoreErrors;
			}
			return newIniValue;
		}

		/// <summary>
		///     Supply the GetSectionDescription
		/// </summary>
		private void GetDescription(MethodCallInfo methodCallInfo)
		{
			var descriptionAttribute = typeof (T).GetCustomAttribute<DescriptionAttribute>();
			if (!string.IsNullOrEmpty(descriptionAttribute?.Description))
			{
				methodCallInfo.ReturnValue = descriptionAttribute.Description;
			}
		}

		/// <summary>
		///     Get a single ini value
		/// </summary>
		private void GetIniValue(MethodCallInfo methodCallInfo)
		{
			IniValue outIniValue;
			if (_iniValues.TryGetValue(methodCallInfo.PropertyNameOf(0), out outIniValue))
			{
				// return IniValue
				methodCallInfo.ReturnValue = outIniValue;
			}
		}


		/// <summary>
		///     Get all the ini values, these are generated and not cached!
		/// </summary>
		private void GetIniValues(MethodCallInfo methodCallInfo)
		{
			// return a linq which loops over all the properties and generates GetIniValues
			// This prevents that the collection can be modified
			methodCallInfo.ReturnValue = _iniValues;
		}

		/// <summary>
		///     Supply the iniSection name
		/// </summary>
		private void GetSectionName(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = !string.IsNullOrEmpty(_iniSectionAttribute?.Name) ? _iniSectionAttribute.Name : typeof (T).Name;
		}

		/// <summary>
		///     Try to get a single ini value
		/// </summary>
		private void TryGetIniValue(MethodCallInfo methodCallInfo)
		{
			IniValue outIniValue;
			methodCallInfo.ReturnValue = _iniValues.TryGetValue(methodCallInfo.PropertyNameOf(0), out outIniValue);
			methodCallInfo.OutArguments = new object[] {outIniValue};
		}
	}
}