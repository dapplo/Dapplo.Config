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
using System.Collections.Generic;
using System.Reflection;
using Dapplo.Config.Attributes;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Intercepting;
using Dapplo.Log;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;
using AutoProperties;

namespace Dapplo.Config
{
    /// <summary>
    /// Genericless base class, making sure we only define the statics once
    /// </summary>
    public abstract class ConfigurationBase
    {
        /// <summary>
        /// The base logged for all the Configuration classes
        /// </summary>
        protected static readonly LogSource Log = new LogSource();

        /// <summary>
        /// Cached values for the GetSetInterceptInformation so this only needs to be calculated once per type
        /// </summary>
        protected static readonly IDictionary<Type, GetSetInterceptInformation> InterceptInformationCache = new Dictionary<Type, GetSetInterceptInformation>();
        /// <summary>
        /// Cached values for the PropertiesInformation so this only needs to be calculated once per type
        /// </summary>
        protected static readonly IDictionary<Type, PropertiesInformation> PropertiesInformationCache = new Dictionary<Type, PropertiesInformation>();
    }

    /// <summary>
    /// An abstract non generic ConfigurationBase.
    /// This defines the API for the configuration based implementations.
    /// If you want to extend the functionality, extend this (or other classes) and implement
    /// a void xxxxxxxGetter(GetInfo) or void xxxxxxxxSetter(SetInfo) which has a InterceptOrderAttribute
    /// </summary>
    public abstract class ConfigurationBase<TProperty> : ConfigurationBase, IShallowCloneable, ITransactionalProperties, IDescription, ITagging, IDefaultValue
    {
        /// <summary>
        /// This is the information for the properties, so we don't need a IDictionary lookup each time
        /// </summary>
        protected PropertiesInformation PropertiesInformation;

        /// <summary>
        /// Information for the interceptors, so we don't need a IDictionary lookup each time
        /// </summary>
        protected GetSetInterceptInformation InterceptInformation;

        #region Interceptor

        /// <summary>
        /// Get the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>TProperty</returns>
        [GetInterceptor]
        public virtual object Getter(string propertyName)
        {
            return GetValue(propertyName).Value;
        }

        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="newValue">object</param>
        [SetInterceptor]
        public virtual void Setter(string propertyName, object newValue)
        {
            SetValue(PropertyInfoFor(propertyName), (TProperty)newValue);
        }
        #endregion

        /// <summary>
        /// Initialize the whole thing, this should be called from the final class
        /// </summary>
        /// <param name="typeToInitializeFor">Type to analyze the properties on</param>
        protected void Initialize(Type typeToInitializeFor)
        {
            var thisType = GetType();
            if (!InterceptInformationCache.TryGetValue(thisType, out InterceptInformation))
            {
                InterceptInformationCache[thisType] = InterceptInformation = new GetSetInterceptInformation(thisType);
            }
            
            if (!PropertiesInformationCache.TryGetValue(typeToInitializeFor, out PropertiesInformation))
            {
                PropertiesInformationCache[thisType] = PropertiesInformation = new PropertiesInformation(typeToInitializeFor);
            }
            // Give extended classes a way to initialize
            foreach (var propertyInfo in PropertiesInformation.PropertyInfos.Values)
            {
                // In theory we could check if the typeToInitializeFor extends ITagging
                InitTagProperty(propertyInfo);
                PropertyInitializer(propertyInfo);
            }
        }

        /// <summary>
        /// This initializes a property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        protected virtual void PropertyInitializer(PropertyInfo propertyInfo)
        {

        }

        /// <summary>
        /// Get all the property names
        /// </summary>
        public IEnumerable<string> PropertyNames() => PropertiesInformation.PropertyInfos.Keys;

        /// <summary>
        /// Try get the PropertyInfo for the specified propertyName
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>bool telling if the try worked</returns>
        protected bool TryGetPropertyInfoFor(string propertyName, out PropertyInfo propertyInfo)
        {
            return PropertiesInformation.PropertyInfos.TryGetValue(propertyName, out propertyInfo);
        }

        /// <summary>
        /// Helper method to get the property info for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns></returns>
        protected PropertyInfo PropertyInfoFor(string propertyName) => PropertiesInformation.PropertyInfoFor(propertyName);

        /// <summary>
        /// This is the internal way of getting information for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>GetInfo</returns>
        protected virtual GetInfo<TProperty> GetValue(string propertyName)
        {
            if (!TryGetPropertyInfoFor(propertyName, out var propertyInfo))
            {
                return null;
            }
            
            return GetValue(propertyInfo);
        }

        /// <summary>
        /// This is the internal way of getting information for a property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>GetInfo</returns>
        protected GetInfo<TProperty> GetValue(PropertyInfo propertyInfo)
        {
            var getInfo = new GetInfo<TProperty>
            {
                PropertyInfo = propertyInfo
            };

            // Call all defined Getter methods the the correct order
            foreach (var methodInfo in InterceptInformation.GetterMethods)
            {
                methodInfo.Invoke(this, new object[] { getInfo });
                if (!getInfo.CanContinue)
                {
                    break;
                }
            }
            return getInfo;
        }


        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="newValue">object</param>
        protected virtual void SetValue(string propertyName, TProperty newValue)
        {
            SetValue(PropertyInfoFor(propertyName), newValue);
        }

        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="newValue">object</param>
        protected virtual void SetValue(PropertyInfo propertyInfo, TProperty newValue)
        {
            propertyInfo.PropertyType.ConvertOrCastValueToType(newValue);

            var setInfo = new SetInfo<TProperty>
            {
                PropertyInfo = propertyInfo,
                NewValue = (TProperty)propertyInfo.PropertyType.ConvertOrCastValueToType(newValue)
            };

            try
            {
                // Call all the defined Setter methods in the correct order
                foreach (var methodInfo in InterceptInformation.SetterMethods)
                {
                    methodInfo.Invoke(this, new object[] {setInfo});
                    if (!setInfo.CanContinue)
                    {
                        break;
                    }
                }
            }
            catch (TargetInvocationException tiEx)
            {
                if (tiEx.InnerException != null)
                {
                    throw tiEx.InnerException;
                }

                throw;
            }
        }

        #region Implementation of ITransactionalProperties
        // A store for the values that are set during the transaction
        private readonly IDictionary<string, TProperty> _transactionProperties = new Dictionary<string, TProperty>(new AbcComparer());
        // This boolean has the value true if we are currently in a transaction
        private bool _inTransaction;

        /// <summary>
        ///     This is the implementation of the getter logic for a transactional proxy
        /// </summary>
        /// <param name="getInfo">GetInfo with all the information on the get call</param>
        [GetSetInterceptor(GetterOrders.Transaction)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void TransactionalGetter(GetInfo<TProperty> getInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            // Lock to prevent rollback etc to run parallel
            lock (_transactionProperties)
            {
                if (!_inTransaction)
                {
                    return;
                }

                // Get the value from the dictionary
                if (!_transactionProperties.TryGetValue(getInfo.PropertyInfo.Name, out var value))
                {
                    return;
                }

                getInfo.HasValue = true;
                getInfo.Value = value;
                getInfo.CanContinue = false;
            }
        }

        /// <summary>
        ///     This is the implementation of the set logic
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information on the set call</param>
        [GetSetInterceptor(SetterOrders.Transaction, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void TransactionalSetter(SetInfo<TProperty> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            // Lock to prevent rollback etc to run parallel
            lock (_transactionProperties)
            {
                if (!_inTransaction)
                {
                    return;
                }

                if (_transactionProperties.TryGetValue(setInfo.PropertyInfo.Name, out var oldValue))
                {
                    _transactionProperties[setInfo.PropertyInfo.Name] = setInfo.NewValue;
                    setInfo.OldValue = oldValue;
                    setInfo.HasOldValue = true;
                }
                else
                {
                    _transactionProperties.Add(setInfo.PropertyInfo.Name, setInfo.NewValue);
                    setInfo.OldValue = default;
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
                    SetValue(PropertiesInformation.PropertyInfoFor(transactionProperty.Key), transactionProperty.Value);
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

        #endregion

        #region Implementation of IDescription
        /// <summary>
        ///     Return the description for a property
        /// </summary>
        public string DescriptionFor(string propertyName)
        {
            return PropertyInfoFor(propertyName).GetDescription();
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
            TProperty defaultValue;
            try
            {
                defaultValue = (TProperty)GetConvertedDefaultValue(propertyInfo);
            }
            catch (Exception ex)
            {
                Log.Warn().WriteLine(ex.Message);
                throw;
            }

            if (defaultValue != null)
            {
                SetValue(propertyInfo, defaultValue);
                return;
            }
            try
            {
                defaultValue = (TProperty)propertyInfo.PropertyType.CreateInstance();
                SetValue(propertyInfo, defaultValue);
            }
            catch (Exception ex)
            {
                // Ignore creating the default type, this might happen if there is no default constructor.
                Log.Warn().WriteLine(ex.Message);
            }
        }

        /// <summary>
        ///     Retrieve the default value, using the TypeConverter
        /// </summary>
        /// <param name="propertyInfo">Property to get the default value for</param>
        /// <returns>object with the type converted default value</returns>
        protected static object GetConvertedDefaultValue(PropertyInfo propertyInfo)
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

        #region Implementation of IShallowCloneable
        /// <inheritdoc />
        public virtual object ShallowClone()
        {
            var clonedValue = Activator.CreateInstance(GetType()) as ConfigurationBase<TProperty>;
            clonedValue?.Initialize(GetType());
            return clonedValue;
        }
        #endregion
    }
}
