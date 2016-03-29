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
using Dapplo.Config.Interceptor.Extensions;
using Dapplo.Config.Interceptor.Implementation;
using Dapplo.Config.Support;
using System.Linq.Expressions;
using System;

#endregion

namespace Dapplo.Config.Ini.Implementation
{
	/// <summary>
	///     Implementation for IIniSection
	/// </summary>
	public class IniSection<T> : ExtensibleInterceptorImpl<T>, IIniSection, IIniSection<T>
	{
		private readonly IniSectionAttribute _iniSectionAttribute = typeof (T).GetCustomAttribute<IniSectionAttribute>();
		private IDictionary<string, IniValue> _iniValues = new Dictionary<string, IniValue>(AbcComparer.Instance);

		/// <summary>
		/// Logic to generate an IniValue for every property
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="extensions"></param>
		protected override void InitProperty(PropertyInfo propertyInfo, IList<IInterceptorExtension> extensions)
		{
			base.InitProperty(propertyInfo, extensions);
			var iniValue = new IniValue(this)
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
			if (!iniValue.Behavior.IsIgnoreErrorsSet && _iniSectionAttribute != null)
			{
				iniValue.Behavior.IgnoreErrors = _iniSectionAttribute.IgnoreErrors;
			}
			_iniValues.Add(iniValue.PropertyName, iniValue);
		}

		protected override void AfterInitialization(IList<IInterceptorExtension> extensions)
		{
			base.AfterInitialization(extensions);
			foreach (var propertyName in InitializationErrors.Keys.ToList())
			{
				IniValue currentValue;
				if (_iniValues.TryGetValue(propertyName, out currentValue))
				{
					if (currentValue.Behavior.IgnoreErrors)
					{
						InitializationErrors.Remove(propertyName);
						continue;
					}
					throw InitializationErrors[propertyName];
				}
			}
		}

		#region IIniSection
		/// <summary>
		/// Get the description for the IniSection
		/// </summary>
		/// <returns>string</returns>
		public string GetSectionDescription()
		{
			var descriptionAttribute = typeof(T).GetCustomAttribute<DescriptionAttribute>();
			return descriptionAttribute?.Description;
		}

		/// <summary>
		/// Indexer for this
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public IniValue this[string propertyName]
		{
			get
			{
				return GetIniValue(propertyName);
			}
		}

		/// <summary>
		/// Get a single ini value via the property name
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>IniValue</returns>
		public IniValue GetIniValue(string propertyName)
		{
			IniValue outIniValue;
			_iniValues.TryGetValue(propertyName, out outIniValue);
			return outIniValue;
		}

		/// <summary>
		/// Get a single ini value via an expression which defines the property name
		/// </summary>
		/// <param name="propertyExpression">LambdaExpression for the property</param>
		/// <returns>IniValue</returns>
		public IniValue GetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression)
		{
			var propertyName = propertyExpression.GetMemberName();
			return GetIniValue(propertyName);
		}


		/// <summary>
		///     Get all the ini values
		/// </summary>
		public IReadOnlyDictionary<string, IniValue> GetIniValues()
		{
			return new ReadOnlyDictionary<string, IniValue>(_iniValues);
		}

		/// <summary>
		/// Supply the iniSection name
		/// </summary>
		/// <returns>string</returns>
		public string GetSectionName()
		{
			return !string.IsNullOrEmpty(_iniSectionAttribute?.Name) ? _iniSectionAttribute.Name : typeof (T).Name;
		}

		/// <summary>
		///     Try to get a single ini value
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="outIniValue">IniValue out</param>
		/// <returns>bool</returns>
		public bool TryGetIniValue(string propertyName, out IniValue outIniValue)
		{
			return _iniValues.TryGetValue(propertyName, out outIniValue);
		}

		/// <summary>
		///     Try to get a single ini value
		/// </summary>
		/// <param name="propertyExpression">LambdaExpression</param>
		/// <param name="outIniValue">IniValue out</param>
		/// <returns>bool</returns>
		public bool TryGetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression, out IniValue iniValue)
		{
			var propertyName = propertyExpression.GetMemberName();
			return TryGetIniValue(propertyName, out iniValue);
		}

		#endregion
	}
}