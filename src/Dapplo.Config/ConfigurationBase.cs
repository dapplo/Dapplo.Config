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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapplo.Config.Interfaces;
using Dapplo.Config.Internal;
using Dapplo.Log;
using Dapplo.Utils;
using Dapplo.Utils.Extensions;

namespace Dapplo.Config
{
    /// <summary>
    /// An abstract non generic ConfigurationBase.
    /// This defines the API for the configuration based implementations
    /// </summary>
    public abstract class ConfigurationBase : ITransactionalProperties //IDescription, , ITagging, IShallowCloneable
    {
        /// <summary>
        /// The base logged for all the Configuration classes
        /// </summary>
        protected static readonly LogSource Log = new LogSource();
        /// <summary>
        /// Store of PropertyInfos for every property
        /// </summary>
        protected readonly IDictionary<string, PropertyInfo> PropertyInfos = new Dictionary<string, PropertyInfo>(AbcComparer.Instance);

        /// <summary>
        /// Store of setter methods
        /// </summary>
        private readonly IDictionary<Type, MethodInfo[]> _setterMethods = new Dictionary<Type, MethodInfo[]>();

        /// <summary>
        /// Store of setter methods
        /// </summary>
        private readonly IDictionary<Type, MethodInfo[]> _getterMethods = new Dictionary<Type, MethodInfo[]>();

        /// <summary>
        /// Initialize the whole thing, this should be called from the final class
        /// </summary>
        /// <param name="typeToInitializeFor">Type to analyze the properties on</param>
        protected void Initialize(Type typeToInitializeFor)
        {
            var thisType = GetType(); //.BaseType;
            var methods = thisType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (!_setterMethods.ContainsKey(thisType))
            {
                _setterMethods[thisType] = methods
                    .Select(methodInfo => new Tuple<MethodInfo, SetterAttribute>(methodInfo, methodInfo.GetAttribute<SetterAttribute>()))
                    .Where(tuple => tuple.Item2 != null)
                    .OrderBy(tuple => tuple.Item2.Order)
                    .Select(tuple => tuple.Item1).ToArray();
            }
            if (!_getterMethods.ContainsKey(thisType))
            {
                _getterMethods[thisType] = methods
                    .Select(methodInfo => new Tuple<MethodInfo, GetterAttribute>(methodInfo, methodInfo.GetAttribute<GetterAttribute>()))
                    .Where(tuple => tuple.Item2 != null)
                    .OrderBy(tuple => tuple.Item2.Order)
                    .Select(tuple => tuple.Item1).ToArray();
            }

            var typeToAnalyze = typeToInitializeFor ?? GetType();
            var types = new[] { typeToAnalyze }.Concat(typeToAnalyze.GetInterfaces());

            foreach (var type in types)
            {
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var propertyName = propertyInfo.Name;
                    if (PropertyInfos.ContainsKey(propertyName))
                    {
                        continue;
                    }
                    if (propertyName == "Item")
                    {
                        continue;
                    }

                    PropertyInfos[propertyName] = propertyInfo;
                }
            }

            // Give extended classes a way to initialize
            foreach (var propertyInfo in PropertyInfos.Values)
            {
                PropertyInitializer(propertyInfo);
            }

        }

        /// <summary>
        /// Helper method to find the PropertyInfo
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>PropertyInfo</returns>
        protected PropertyInfo PropertyInfoFor(string propertyName)
        {
            if (!PropertyInfos.TryGetValue(propertyName, out var propertyInfo))
            {
                throw new NotSupportedException($"Property {propertyName} doesn't exist.");
            }

            return propertyInfo;
        }

        /// <summary>
        /// This initializes a property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        protected virtual void PropertyInitializer(PropertyInfo propertyInfo)
        {

        }

        /// <inheritdoc />
        public virtual object ShallowClone()
        {
            var clonedValue = Activator.CreateInstance(GetType()) as ConfigurationBase;
            clonedValue?.Initialize(GetType());
            return clonedValue;
        }


        /// <summary>
        /// This is the internal way of getting information for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns></returns>
        protected GetInfo GetValue(string propertyName)
        {
            var propertyInfo = PropertyInfoFor(propertyName);

            var getInfo = new GetInfo
            {
                PropertyInfo = propertyInfo
            };

            var getters = _getterMethods[GetType()];
            foreach (var methodInfo in getters)
            {
                methodInfo.Invoke(this, new object[] {getInfo});
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
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="newValue">object</param>
        protected void SetValue(PropertyInfo propertyInfo, object newValue)
        {
            propertyInfo.PropertyType.ConvertOrCastValueToType(newValue);

            var setInfo = new SetInfo
            {
                PropertyInfo = propertyInfo,
                NewValue = propertyInfo.PropertyType.ConvertOrCastValueToType(newValue)
            };

            var setters = _setterMethods[GetType()];
            try
            {
                foreach (var methodInfo in setters)
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
        private readonly IDictionary<string, object> _transactionProperties = new Dictionary<string, object>(new AbcComparer());
        // This boolean has the value true if we are currently in a transaction
        private bool _inTransaction;

        /// <summary>
        ///     This is the implementation of the getter logic for a transactional proxy
        /// </summary>
        /// <param name="getInfo">GetInfo with all the information on the get call</param>
        [Getter(GetterOrders.Transaction)]
        protected void TransactionalGetter(GetInfo getInfo)
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
        [Setter(SetterOrders.Transaction)]
        protected void TransactionalSetter(SetInfo setInfo)
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
                    SetValue(PropertyInfoFor(transactionProperty.Key), transactionProperty.Value);
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

    }
}
