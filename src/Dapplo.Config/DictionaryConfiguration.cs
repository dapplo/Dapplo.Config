﻿// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
    public class DictionaryConfiguration<TInterface> : DictionaryConfigurationBase<TInterface, object>
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
    public class DictionaryConfigurationBase<TInterface, TProperty> : ConfigurationBase<TProperty>, IConfiguration<TProperty>
    {
        private IDictionary<string, TProperty> _properties;

        /// <inheritdoc />
        protected DictionaryConfigurationBase()
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

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        [GetSetInterceptor(SetterOrders.WriteProtect, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void WriteProtectSetter(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
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


        private bool _trackChanges;
        // This boolean has the value true if we have changes since the last "reset"
        private readonly ISet<string> _changedValues = new HashSet<string>(new AbcComparer());

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        [GetSetInterceptor(SetterOrders.HasChanges, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void HasChangesSetter(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (!_trackChanges)
            {
                return;
            }

            if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
            {
                _changedValues.Add(setInfo.PropertyInfo.Name.ToLowerInvariant());
            }
        }

        /// <inheritdoc />
        public void TrackChanges()
        {
            _trackChanges = true;
        }

        /// <inheritdoc />
        public void DoNotTrackChanges()
        {
            _trackChanges = false;
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
        [GetSetInterceptor(SetterOrders.NotifyPropertyChanged, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void NotifyPropertyChangedSetter(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
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
            var propertyChangedEventArgs = new PropertyChangedEventArgsEx(setInfo.PropertyInfo.Name, setInfo.OldValue, setInfo.NewValue);
            InvokePropertyChanged(Proxy, propertyChangedEventArgs);
        }

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
        [GetSetInterceptor(SetterOrders.NotifyPropertyChanging, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void NotifyPropertyChangingSetter(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
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
            var propertyChangingEventArgs = new PropertyChangingEventArgsEx(setInfo.PropertyInfo.Name, setInfo.OldValue, setInfo.NewValue);
            InvokePropertyChanging(Proxy, propertyChangingEventArgs);
        }

        /// <inheritdoc />
        public override object ShallowClone()
        {
            var type = GetType();
            var clonedValue = (DictionaryConfigurationBase<TInterface, TProperty>) Activator.CreateInstance(type);
            clonedValue.SetProperties(_properties);
            return ConfigProxy.Create<TInterface>(clonedValue);
        }
    }
}
