//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
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
using System.Linq.Expressions;
using System.Reflection;
using AutoProperties;
using Dapplo.Config.Attributes;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Intercepting;
using Dapplo.Log;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

namespace Dapplo.Config
{
    /// <summary>
    /// ConfigBase is a generic abstract configuration class
    /// </summary>
    /// <typeparam name="T">The type of the configuration interface this class implements</typeparam>
    public abstract class DictionaryConfigurationBase<T> : ConfigurationBase, IConfiguration
    {
        private IDictionary<string, object> _properties;

        /// <inheritdoc />
        public DictionaryConfigurationBase()
        {
            _properties = new ConcurrentDictionary<string, object>(AbcComparer.Instance);
            Initialize(typeof(T));
        }

        /// <summary>
        /// A way to specify the property values which need to be used
        /// </summary>
        /// <param name="properties">IDictionary with the properties to use</param>
        protected DictionaryConfigurationBase(IDictionary<string, object> properties)
        {
            _properties = new ConcurrentDictionary<string, object>(properties, AbcComparer.Instance);
            Initialize(typeof(T));
        }

        /// <summary>
        /// Used for cloning or resetting values
        /// </summary>
        /// <param name="properties"></param>
        protected void SetProperties(IDictionary<string, object> properties)
        {
            _properties = new ConcurrentDictionary<string, object>(properties, AbcComparer.Instance);
            foreach (var propertyInfo in Information.PropertyInfos.Values)
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
        /// <param name="key">string with key for the property to get</param>
        /// <returns>object or null if not available</returns>
        public virtual object this[string key]
        {
            get => Getter(key);
            set => Setter(key, value);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, object>> GetProperties() => _properties;

        /// <summary>
        /// Retrieves the value from the dictionary
        /// </summary>
        /// <param name="getInfo">GetInfo</param>
        [Getter(GetterOrders.Dictionary)]
        protected void ValueFromDictionary(GetInfo getInfo)
        {
            var hasValue = _properties.TryGetValue(getInfo.PropertyInfo.Name, out var value);
            getInfo.Value = hasValue ? value : GetConvertedDefaultValue(getInfo.PropertyInfo);
            getInfo.HasValue = hasValue;
        }

        /// <summary>
        /// Retrieves the value from the dictionary
        /// </summary>
        /// <param name="setInfo">GetInfo</param>
        [Setter(SetterOrders.Dictionary)]
        protected void ValueToDictionary(SetInfo setInfo)
        {
            _properties[setInfo.PropertyInfo.Name] = setInfo.NewValue;
        }

        #region Interceptor

        /// <summary>
        /// Get the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>TProperty</returns>
        [GetInterceptor]
        protected virtual object Getter(string propertyName)
        {
            return GetValue(propertyName).Value;
        }

        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="newValue">object</param>
        [SetInterceptor]
        protected virtual void Setter(string propertyName, object newValue)
        {
            SetValue(PropertyInfoFor(propertyName), newValue);
        }
        #endregion

        #region Implementation of IWriteProtectProperties

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        [Setter(SetterOrders.WriteProtect)]
        protected void WriteProtectSetter(SetInfo setInfo)
        {
            if (_writeProtectedProperties.Contains(setInfo.PropertyInfo.Name))
            {
                setInfo.CanContinue = false;
                throw new AccessViolationException($"Property {setInfo.PropertyInfo.Name} is write protected");
            }
            if (_isProtecting)
            {
                _writeProtectedProperties.Add(setInfo.PropertyInfo.Name);
            }
        }

        // A store for the values that are write protected
        private readonly ISet<string> _writeProtectedProperties = new HashSet<string>(AbcComparer.Instance);
        private bool _isProtecting;

        /// <inheritdoc />
        public void DisableWriteProtect(string propertyName)
        {
            _writeProtectedProperties.Remove(propertyName);
        }

        /// <inheritdoc />
        public bool IsWriteProtected(string propertyName)
        {
            return _writeProtectedProperties.Contains(propertyName);
        }

        /// <inheritdoc />
        public void RemoveWriteProtection()
        {
            _isProtecting = false;
            _writeProtectedProperties.Clear();
        }

        /// <inheritdoc />
        public void StartWriteProtecting()
        {
            _isProtecting = true;
        }

        /// <inheritdoc />
        public void StopWriteProtecting()
        {
            _isProtecting = false;
        }

        /// <inheritdoc />
        public void WriteProtect(string propertyName)
        {
            _writeProtectedProperties.Add(propertyName);
        }

        /// <inheritdoc />
        public void DisableWriteProtect<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            DisableWriteProtect(propertyExpression.GetMemberName());
        }

        /// <inheritdoc />
        public bool IsWriteProtected<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            return IsWriteProtected(propertyExpression.GetMemberName());
        }

        /// <inheritdoc />
        public void WriteProtect<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            WriteProtect(propertyExpression.GetMemberName());
        }

        #endregion

        #region Implementation of IHasChanges
        // This boolean has the value true if we have changes since the last "reset"
        private readonly ISet<string> _changedValues = new HashSet<string>(new AbcComparer());

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        [Setter(SetterOrders.HasChanges)]
        protected void HasChangesSetter(SetInfo setInfo)
        {
            var hasOldValue = _properties.TryGetValue(setInfo.PropertyInfo.Name, out var oldValue);
            setInfo.HasOldValue = hasOldValue;
            setInfo.OldValue = oldValue;

            if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
            {
                _changedValues.Add(setInfo.PropertyInfo.Name.ToLowerInvariant());
            }
        }

        /// <inheritdoc />
        public bool IsChanged<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            return IsChanged(propertyExpression.GetMemberName());
        }

        /// <inheritdoc />
        public bool HasChanges()
        {
            return _changedValues.Count > 0;
        }

        /// <inheritdoc />
        public void ResetHasChanges()
        {
            _changedValues.Clear();
        }

        /// <inheritdoc />
        public ISet<string> Changes()
        {
            return new HashSet<string>(_changedValues, new AbcComparer());
        }

        /// <inheritdoc />
        public bool IsChanged(string propertyName)
        {
            return _changedValues.Contains(propertyName);
        }

        #endregion



        #region Implementation of INotifyPropertyChanged

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        ///     This is the logic which is called to invoke the event.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="eventArgs">PropertyChangedEventArgs</param>
        private void InvokePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(sender, eventArgs);
        }

        /// <summary>
        ///     This creates a NPC event if the values are changed
        /// </summary>
        /// <param name="setInfo">SetInfo with all the set call information</param>
        [Setter(SetterOrders.NotifyPropertyChanged)]
        protected void NotifyPropertyChangedSetter(SetInfo setInfo)
        {
            // Fast exit when no listeners.
            if (PropertyChanged is null)
            {
                return;
            }
            // Create the event if the property changed
            if (setInfo.HasOldValue && Equals(setInfo.NewValue, setInfo.OldValue))
            {
                return;
            }
            var propertyChangedEventArgs = new PropertyChangedEventArgs(setInfo.PropertyInfo.Name);
            InvokePropertyChanged(this, propertyChangedEventArgs);
        }

        #endregion

        #region Implementation of INotifyPropertyChanging

        /// <inheritdoc />
        public event PropertyChangingEventHandler PropertyChanging;


        /// <summary>
        ///     This is the logic which is called to invoke the event.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="eventArgs">PropertyChangingEventArgs</param>
        private void InvokePropertyChanging(object sender, PropertyChangingEventArgs eventArgs)
        {
            PropertyChanging?.Invoke(sender, eventArgs);
        }

        /// <summary>
        ///     This creates a NPC event if the values are changing
        /// </summary>
        /// <param name="setInfo">SetInfo with all the set call information</param>
        [Setter(SetterOrders.NotifyPropertyChanging)]
        protected void NotifyPropertyChangingSetter(SetInfo setInfo)
        {
            if (PropertyChanging is null)
            {
                return;
            }
            // Create the event if the property is changing
            if (setInfo.HasOldValue && Equals(setInfo.NewValue, setInfo.OldValue))
            {
                return;
            }
            var propertyChangingEventArgs = new PropertyChangingEventArgs(setInfo.PropertyInfo.Name);
            InvokePropertyChanging(this, propertyChangingEventArgs);
        }
        #endregion

        #region Implementation of IShallowCloneable

        /// <inheritdoc />
        public override object ShallowClone()
        {
            var type = GetType();
            var clonedValue = (DictionaryConfigurationBase<T>) Activator.CreateInstance(type);
            clonedValue.SetProperties(_properties);
            return clonedValue;
        }

        #endregion
    }
}
