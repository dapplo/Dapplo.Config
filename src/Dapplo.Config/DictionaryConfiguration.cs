//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Dapplo.Config.Attributes;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Intercepting;

namespace Dapplo.Config
{

    /// <summary>
    /// DictionaryConfiguration is a IDictionary based configuration store
    /// </summary>
    /// <typeparam name="TInterface">The type of the configuration interface this class implements</typeparam>
    public class DictionaryConfiguration<TInterface> : DictionaryConfiguration<TInterface, object>
    {
        /// <summary>
        /// Factory for DictionaryConfiguration implementations
        /// </summary>
        /// <returns>TInterface</returns>
        public static TInterface Create()
        {
            return ConfigProxy.Create<TInterface>(new DictionaryConfiguration<TInterface>());
        }
    }

    /// <summary>
    /// DictionaryConfiguration is a IDictionary based configuration store
    /// </summary>
    /// <typeparam name="TInterface">The type of the configuration interface this class implements</typeparam>
    /// <typeparam name="TProperty">The type of the property value</typeparam>
    public class DictionaryConfiguration<TInterface, TProperty> : ConfigurationBase<TProperty>, IConfiguration<TProperty>
    {
        private IDictionary<string, TProperty> _properties;

        /// <inheritdoc />
        protected DictionaryConfiguration()
        {
            _properties = new ConcurrentDictionary<string, TProperty>(AbcComparer.Instance);
            Initialize(typeof(TInterface));
        }

        /// <summary>
        /// Used for cloning or resetting values
        /// </summary>
        /// <param name="properties"></param>
        protected void SetProperties(IDictionary<string, TProperty> properties)
        {
            _properties = new ConcurrentDictionary<string, TProperty>(properties, AbcComparer.Instance);
            foreach (var propertyInfo in PropertiesInformation.PropertyInfos.Values)
            {
                if (_properties.TryGetValue(propertyInfo.Name, out var value) && value != null)
                {
                    continue;
                }
                // Set a default
                RestoreToDefault(propertyInfo.Name);
            }
        }

        /// <summary>
        /// This is only called when the type is initially created, per property.
        /// The main use case for this, is to build caches or process attributes on properties
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        protected override void PropertyInitializer(PropertyInfo propertyInfo)
        {
            // Set a default
            RestoreToDefault(propertyInfo.Name);
        }

        /// <summary>
        ///     Get the value for a property.
        /// Note: This needs to be virtual otherwise the interface isn't implemented
        /// </summary>
        /// <param name="propertyName">string with key for the property to get</param>
        /// <returns>object or null if not available</returns>
        public virtual TProperty this[string propertyName]
        {
            get => GetValue(propertyName).Value;
            set => SetValue(propertyName, value);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, TProperty>> Properties() => _properties;

        /// <summary>
        /// Retrieves the value from the dictionary
        /// </summary>
        /// <param name="getInfo">GetInfo</param>
        [GetSetInterceptor(GetterOrders.Dictionary)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void FromDictionaryGetter(GetInfo<TProperty> getInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var hasValue = _properties.TryGetValue(getInfo.PropertyInfo.Name, out var value);
            getInfo.Value = hasValue ? value : (TProperty)GetConvertedDefaultValue(getInfo.PropertyInfo);
            getInfo.HasValue = hasValue;
        }

        /// <summary>
        /// Make sure the SetInfo is correctly filled
        /// </summary>
        /// <param name="setInfo">SetInfo</param>
        [GetSetInterceptor(SetterOrders.SetInfoInitializer, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void SetInfoInitializer(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var hasOldValue = _properties.TryGetValue(setInfo.PropertyInfo.Name, out var oldValue);
            setInfo.HasOldValue = hasOldValue;
            setInfo.OldValue = oldValue;
        }

        /// <summary>
        /// Retrieves the value from the dictionary
        /// </summary>
        /// <param name="setInfo">SetInfo</param>
        [GetSetInterceptor(SetterOrders.Dictionary, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void ToDictionarySetter(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            _properties[setInfo.PropertyInfo.Name] = setInfo.NewValue;
        }

        #region Implementation of IShallowCloneable

        /// <inheritdoc />
        public override object ShallowClone()
        {
            var type = GetType();
            var clonedValue = (DictionaryConfiguration<TInterface, TProperty>) Activator.CreateInstance(type);
            clonedValue.SetProperties(_properties);
            return ConfigProxy.Create<TInterface>(clonedValue);
        }

        #endregion
    }
}
