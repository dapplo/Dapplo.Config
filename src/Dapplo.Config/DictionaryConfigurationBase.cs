//  Dapplo - building blocks for desktop applications
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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using AutoProperties;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Internal;
using Dapplo.Log;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

namespace Dapplo.Config
{
    /// <summary>
    /// ConfigBase is a generic abstract configuration class
    /// </summary>
    /// <typeparam name="T">The type of the configuration interface this class implements</typeparam>
    public abstract class DictionaryConfigurationBase<T> : ConfigurationBase, IConfiguration<T>
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
            foreach (var propertyInfo in PropertyInfos.Values)
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
            // Retrieve the tags
            InitTagProperty(propertyInfo);

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
            set => Setter(PropertyInfoFor(key), value);
        }

        /// <inheritdoc />
        public IEnumerable<KeyValuePair<string, object>> GetProperties() => _properties;

        #region Interceptor

        /// <summary>
        /// This is the internal way of getting information for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns></returns>
        protected GetInfo GetValue(string propertyName)
        {
            var propertyInfo = PropertyInfoFor(propertyName);

            var hasValue = _properties.TryGetValue(propertyName, out var value);
            var getInfo = new GetInfo
            {
                PropertyName = propertyName,
                PropertyType = propertyInfo.PropertyType,
                Value = hasValue ? value : GetConvertedDefaultValue(propertyInfo),
                HasValue = hasValue
            };

            TransactionalGetter(getInfo);
            // If more getters need to be called, check getInfo.CanContinue before calling them

            return getInfo;
        }

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
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="newValue">object</param>
        protected void Setter(PropertyInfo propertyInfo, object newValue)
        {
            var hasOldValue = _properties.TryGetValue(propertyInfo.Name, out var oldValue);

            propertyInfo.PropertyType.ConvertOrCastValueToType(newValue);

            var setInfo = new SetInfo
            {
                PropertyName = propertyInfo.Name,
                PropertyType = propertyInfo.PropertyType,
                NewValue = propertyInfo.PropertyType.ConvertOrCastValueToType(newValue),
                HasOldValue = hasOldValue,
                OldValue = oldValue
            };

            // Handle write protection
            WriteProtectSetter(setInfo);
            // Handle transactions
            if (setInfo.CanContinue)
            {
                TransactionalSetter(setInfo);
            }

            // Handle changes
            if (setInfo.CanContinue)
            {
                HasChangesSetter(setInfo);
            }

            // Handle NotifyPropertyChang(ed/ing) and set the value
            if (setInfo.CanContinue)
            {
                NotifyPropertyChangingSetter(setInfo);
                _properties[propertyInfo.Name] = setInfo.NewValue;
                NotifyPropertyChangedSetter(setInfo);
            }
        }

        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="newValue">object</param>
        [SetInterceptor]
        protected virtual void Setter(string propertyName, object newValue)
        {
            Setter(PropertyInfoFor(propertyName), newValue);
        }
        #endregion

        #region Implementation of IDescription
        /// <summary>
        ///     Return the description for a property
        /// </summary>
        public string DescriptionFor(string propertyName)
        {
            return PropertyInfoFor(propertyName).GetDescription();
        }

        /// <summary>
        ///     Return the description for a property
        /// </summary>
        public string DescriptionFor<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            return DescriptionFor(propertyExpression.GetMemberName());
        }

        #endregion

        #region Implementation of IWriteProtectProperties

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        private void WriteProtectSetter(SetInfo setInfo)
        {
            if (_writeProtectedProperties.Contains(setInfo.PropertyName))
            {
                setInfo.CanContinue = false;
                throw new AccessViolationException($"Property {setInfo.PropertyName} is write protected");
            }
            if (_isProtecting)
            {
                _writeProtectedProperties.Add(setInfo.PropertyName);
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
        private void HasChangesSetter(SetInfo setInfo)
        {
            if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
            {
                _changedValues.Add(setInfo.PropertyName.ToLowerInvariant());
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

        #region Implementation of IDefaultValue

        /// <inheritdoc />
        public object DefaultValueFor(string propertyName)
        {
            return GetConvertedDefaultValue(PropertyInfoFor(propertyName));
        }

        /// <inheritdoc />
        public void RestoreToDefault(string propertyName)
        {
            var propertyInfo = PropertyInfoFor(propertyName);
            object defaultValue;
            try
            {
                defaultValue = GetConvertedDefaultValue(propertyInfo);
            }
            catch (Exception ex)
            {
                Log.Warn().WriteLine(ex.Message);
                throw;
            }

            if (defaultValue != null)
            {
                Setter(propertyInfo, defaultValue);
                return;
            }
            try
            {
                defaultValue = propertyInfo.PropertyType.CreateInstance();
                Setter(propertyInfo, defaultValue);
            }
            catch (Exception ex)
            {
                // Ignore creating the default type, this might happen if there is no default constructor.
                Log.Warn().WriteLine(ex.Message);
            }
        }

        /// <inheritdoc />
        public object DefaultValueFor<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            return DefaultValueFor(propertyExpression.GetMemberName());
        }

        /// <inheritdoc />
        public void RestoreToDefault<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            RestoreToDefault(propertyExpression.GetMemberName());
        }

        /// <summary>
        ///     Retrieve the default value, using the TypeConverter
        /// </summary>
        /// <param name="propertyInfo">Property to get the default value for</param>
        /// <returns>object with the type converted default value</returns>
        private static object GetConvertedDefaultValue(PropertyInfo propertyInfo)
        {
            var defaultValue = propertyInfo.GetDefaultValue();
            if (defaultValue != null)
            {
                var typeConverter = propertyInfo.GetTypeConverter();
                var targetType = propertyInfo.PropertyType;
                defaultValue = targetType.ConvertOrCastValueToType(defaultValue, typeConverter);
            }
            return defaultValue;
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
        private void NotifyPropertyChangedSetter(SetInfo setInfo)
        {
            // Fast exit when no listeners.
            if (PropertyChanged == null)
            {
                return;
            }
            // Create the event if the property changed
            if (setInfo.HasOldValue && Equals(setInfo.NewValue, setInfo.OldValue))
            {
                return;
            }
            var propertyChangedEventArgs = new PropertyChangedEventArgs(setInfo.PropertyName);
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
        private void NotifyPropertyChangingSetter(SetInfo setInfo)
        {
            if (PropertyChanging == null)
            {
                return;
            }
            // Create the event if the property is changing
            if (setInfo.HasOldValue && Equals(setInfo.NewValue, setInfo.OldValue))
            {
                return;
            }
            var propertyChangingEventArgs = new PropertyChangingEventArgs(setInfo.PropertyName);
            InvokePropertyChanging(this, propertyChangingEventArgs);
        }
        #endregion

        #region Implementation of ITransactionalProperties
        // A store for the values that are set during the transaction
        private readonly IDictionary<string, object> _transactionProperties = new Dictionary<string, object>(new AbcComparer());
        // This boolean has the value true if we are currently in a transaction
        private bool _inTransaction;

        /// <summary>
        ///     This is the implementation of the getter logic for a transactional proxy
        /// </summary>
        /// <param name="getInfo">GetInfo with all the information on the get call</param>
        private void TransactionalGetter(GetInfo getInfo)
        {
            // Lock to prevent rollback etc to run parallel
            lock (_transactionProperties)
            {
                if (!_inTransaction)
                {
                    return;
                }

                // Get the value from the dictionary
                if (!_transactionProperties.TryGetValue(getInfo.PropertyName, out var value))
                {
                    return;
                }

                getInfo.Value = value;
                getInfo.CanContinue = false;
            }
        }

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        private void TransactionalSetter(SetInfo setInfo)
        {
            // Lock to prevent rollback etc to run parallel
            lock (_transactionProperties)
            {
                if (!_inTransaction)
                {
                    return;
                }

                if (_transactionProperties.TryGetValue(setInfo.PropertyName, out var oldValue))
                {
                    _transactionProperties[setInfo.PropertyName] = setInfo.NewValue;
                    setInfo.OldValue = oldValue;
                    setInfo.HasOldValue = true;
                }
                else
                {
                    _transactionProperties.Add(setInfo.PropertyName, setInfo.NewValue);
                    setInfo.OldValue = null;
                    setInfo.HasOldValue = false;
                }

                // No more (prevents NPC)
                setInfo.CanContinue = false;
            }
        }

        /// <inheritdoc />
        public void CommitTransaction()
        {
            // Lock to prevent rollback etc to run parallel
            lock (_transactionProperties)
            {
                // Only when we have started a transaction
                if (!_inTransaction)
                {
                    return;
                }
                // Disable the transaction, otherwise the set will only overwrite the value in _transactionProperties
                _inTransaction = false;
                // Call the set for every property, this will invoke every setter (NPC etc)
                foreach (var transactionProperty in _transactionProperties)
                {
                    Setter(PropertyInfoFor(transactionProperty.Key), transactionProperty.Value);
                }
                // Clear all the properties, so the transaction is clean
                _transactionProperties.Clear();
            }
        }

        /// <inheritdoc />
        public bool IsTransactionDirty()
        {
            lock (_transactionProperties)
            {
                return _inTransaction && _transactionProperties.Count > 0;
            }
        }

        /// <inheritdoc />
        public void RollbackTransaction()
        {
            // Lock to prevent commit etc to run parallel
            lock (_transactionProperties)
            {
                // Only when we have started a transaction, it can be cleared
                if (!_inTransaction)
                {
                    return;
                }
                _transactionProperties.Clear();
                _inTransaction = false;
            }
        }

        /// <inheritdoc />
        public void StartTransaction()
        {
            lock (_transactionProperties)
            {
                _inTransaction = true;
            }
        }

        #endregion

        #region Implementation of ITagging
        // The set of tagged properties
        private readonly IDictionary<string, IDictionary<object, object>> _taggedProperties = new Dictionary<string, IDictionary<object, object>>(new AbcComparer());

        /// <summary>
        ///     Process the property, in our case get the tags
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        private void InitTagProperty(PropertyInfo propertyInfo)
        {
            foreach (var tagAttribute in propertyInfo.GetAttributes<TagAttribute>())
            {
                if (!_taggedProperties.TryGetValue(propertyInfo.Name, out var tags))
                {
                    tags = new Dictionary<object, object>();
                    _taggedProperties.Add(propertyInfo.Name, tags);
                }
                tags[tagAttribute.Tag] = tagAttribute.TagValue;
            }
        }

        /// <inheritdoc />
        public object GetTagValue(string propertyName, object tag)
        {
            if (!_taggedProperties.TryGetValue(propertyName, out var tags))
            {
                return null;
            }
            var hasTag = tags.ContainsKey(tag);
            object returnValue = null;
            if (hasTag)
            {
                returnValue = tags[tag];
            }
            return returnValue;
        }

        /// <inheritdoc />
        public bool IsTaggedWith(string propertyName, object tag)
        {
            if (_taggedProperties.TryGetValue(propertyName, out var tags))
            {
                return tags.ContainsKey(tag);
            }

            return false;
        }

        /// <inheritdoc />
        public object GetTagValue<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag)
        {
            return GetTagValue(propertyExpression.GetMemberName(), tag);
        }

        /// <inheritdoc />
        public bool IsTaggedWith<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag)
        {
            return IsTaggedWith(propertyExpression.GetMemberName(), tag);
        }

        #endregion

        #region Implementation of IShallowCloneable

        /// <inheritdoc />
        public override object ShallowClone()
        {
            var type = GetType();
            var clonedValue = Activator.CreateInstance(type) as DictionaryConfigurationBase<T>;
            clonedValue.SetProperties(_properties);
            return clonedValue;
        }

        #endregion
    }
}
