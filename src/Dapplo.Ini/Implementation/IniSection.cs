﻿//  Dapplo - building blocks for desktop applications
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Ini.Extensions;
using Dapplo.InterfaceImpl;
using Dapplo.InterfaceImpl.Implementation;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

#endregion

namespace Dapplo.Ini.Implementation
{
	/// <summary>
	///     Implementation for IIniSection
	/// </summary>
	public class IniSection<T> : ExtensibleInterceptorImpl<T>, IIniSection<T>, IIniSectionInternal
	{
		private readonly IniSectionAttribute _iniSectionAttribute = typeof(T).GetAttribute<IniSectionAttribute>();
		private readonly IDictionary<string, IniValue> _iniValues = new Dictionary<string, IniValue>(AbcComparer.Instance);

		/// <summary>
		///     This is called by the ExtensibleInterceptorImpl with a list of extensions
		/// </summary>
		/// <param name="extensions">list of extensions</param>
		protected override void AfterInitialization(IEnumerable<IInterceptorExtension> extensions)
		{
			base.AfterInitialization(extensions);
			foreach (var propertyName in InitializationErrors.Keys.ToList())
			{
				if (!_iniValues.TryGetValue(propertyName, out var currentValue))
				{
					continue;
				}
				if (!currentValue.Behavior.IgnoreErrors)
				{
					throw InitializationErrors[propertyName];
				}
				InitializationErrors.Remove(propertyName);
			}
		}

		/// <summary>
		///     Logic to generate an IniValue for every property
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="extensions"></param>
		protected override void InitProperty(PropertyInfo propertyInfo, IEnumerable<IInterceptorExtension> extensions)
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

		#region Events

		/// <summary>
		///     This is the reset event
		/// </summary>
		public event EventHandler<IniSectionEventArgs> Reset;

		/// <summary>
		///     This is the loaded event
		/// </summary>
		public event EventHandler<IniSectionEventArgs> Loaded;

		/// <summary>
		///     This is the saved event
		/// </summary>
		public event EventHandler<IniSectionEventArgs> Saved;

		/// <summary>
		///     This is the saving event
		/// </summary>
		public event EventHandler<IniSectionEventArgs> Saving;

		void IIniSectionInternal.OnReset()
		{
			Reset?.Invoke(this, new IniSectionEventArgs
			{
				EventType = IniEventTypes.Reset,
				IniSection = this
			});
		}

		void IIniSectionInternal.OnLoaded()
		{
			Loaded?.Invoke(this, new IniSectionEventArgs
			{
				EventType = IniEventTypes.Loaded,
				IniSection = this
			});
		}

		void IIniSectionInternal.OnSaved()
		{
			Saved?.Invoke(this, new IniSectionEventArgs
			{
				EventType = IniEventTypes.Saved,
				IniSection = this
			});
		}

		void IIniSectionInternal.OnSaving()
		{
			Saving?.Invoke(this, new IniSectionEventArgs
			{
				EventType = IniEventTypes.Saving,
				IniSection = this
			});
		}

		#endregion

		#region IIniSection

		/// <summary>
		///     Get the description for the IniSection
		/// </summary>
		/// <returns>string</returns>
		public string GetSectionDescription()
		{
			var descriptionAttribute = typeof(T).GetCustomAttribute<DescriptionAttribute>();
			return descriptionAttribute?.Description;
		}

		/// <summary>
		///     Implementation of the indexer of the IIniSection
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>IniValue</returns>
		IniValue IIniSection.this[string propertyName] => GetIniValue(propertyName);

		/// <summary>
		///     Get a single ini value via the property name
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>IniValue</returns>
		public IniValue GetIniValue(string propertyName)
		{
			_iniValues.TryGetValue(propertyName, out var outIniValue);
			return outIniValue;
		}

		/// <summary>
		///     Get a single ini value via an expression which defines the property name
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
		///     Supply the iniSection name
		/// </summary>
		/// <returns>string</returns>
		public string GetSectionName()
		{
			return !string.IsNullOrEmpty(_iniSectionAttribute?.Name) ? _iniSectionAttribute.Name : typeof(T).Name;
		}

		/// <summary>
		///     Try to get a single ini value
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="value">IniValue out</param>
		/// <returns>bool</returns>
		public bool TryGetIniValue(string propertyName, out IniValue value)
		{
			return _iniValues.TryGetValue(propertyName, out value);
		}

		/// <summary>
		///     Try to get a single ini value
		/// </summary>
		/// <param name="propertyExpression">LambdaExpression</param>
		/// <param name="value">IniValue out</param>
		/// <returns>bool</returns>
		public bool TryGetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression, out IniValue value)
		{
			var propertyName = propertyExpression.GetMemberName();
			return TryGetIniValue(propertyName, out value);
		}

		#endregion
	}
}