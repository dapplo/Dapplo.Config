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
using Dapplo.Config.Intercepting;
using Dapplo.Config.Extensions;
using Dapplo.Log;

namespace Dapplo.Config
{
    /// <summary>
    /// Generic-less base class, solving an issue that static members are created per generic type
    /// </summary>
    public abstract class ConfigurationBase
    {
        /// <summary>
        /// The base logged for all the Configuration classes
        /// </summary>
        protected static readonly LogSource Log = new LogSource();

        /// <summary>
        /// This is the proxy on which the user code is operating
        /// </summary>
        public ConfigProxy Proxy { get; internal set; }

        public Type ProxyFor { get; internal set; }

        /// <summary>
        /// This is the information for the properties, so we don't need a IDictionary lookup each time
        /// </summary>
        public PropertiesInformation PropertiesInformation;

        /// <summary>
        /// Information for the interceptors, so we don't need a IDictionary lookup each time
        /// </summary>
        protected GetSetInterceptInformation InterceptInformation;

        /// <summary>
        /// Cached values for the GetSetInterceptInformation so this only needs to be calculated once per type
        /// </summary>
        protected static readonly IDictionary<Type, GetSetInterceptInformation> InterceptInformationCache = new Dictionary<Type, GetSetInterceptInformation>();
        /// <summary>
        /// Cached values for the PropertiesInformation so this only needs to be calculated once per type
        /// </summary>
        protected static readonly IDictionary<Type, PropertiesInformation> PropertiesInformationCache = new Dictionary<Type, PropertiesInformation>();

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
        public bool TryGetPropertyInfoFor(string propertyName, out PropertyInfo propertyInfo)
        {
            return PropertiesInformation.PropertyInfos.TryGetValue(propertyName, out propertyInfo);
        }

        /// <summary>
        /// Helper method to get the property info for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns></returns>
        public PropertyInfo PropertyInfoFor(string propertyName) => PropertiesInformation.PropertyInfoFor(propertyName);

        /// <summary>
        /// Get the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>TProperty</returns>
        public abstract object Getter(string propertyName);

        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="newValue">object</param>
        public abstract void Setter(string propertyName, object newValue);
    }

    /// <summary>
    /// An abstract non generic ConfigurationBase.
    /// This defines the API for the configuration based implementations.
    /// If you want to extend the functionality, extend this (or other classes) and implement
    /// a void xxxxxxxxGetter(GetInfo) or void xxxxxxxxSetter(SetInfo) which has a InterceptOrderAttribute
    /// </summary>
    public abstract class ConfigurationBase<TProperty> : ConfigurationBase
    {
        /// <inheritdoc />
        public override object Getter(string propertyName)
        {
            var getInfo = GetValue(propertyName);
            if (getInfo == null)
            {
                return null;
            }
            return getInfo.Value;
        }

        /// <inheritdoc />
        public override void Setter(string propertyName, object newValue)
        {
            SetValue(PropertyInfoFor(propertyName), (TProperty)newValue);
        }

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
        /// This is the internal way of getting information for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>GetInfo</returns>
        public virtual GetInfo<TProperty> GetValue(string propertyName)
        {
            if (TryGetPropertyInfoFor(propertyName, out var propertyInfo))
            {
                return GetValue(propertyInfo);
            }
            Log.Warn().WriteLine("Couldn't find a property called {0}", propertyName);
            return null;
        }

        /// <summary>
        /// This is the internal way of getting information for a property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <returns>GetInfo</returns>
        public GetInfo<TProperty> GetValue(PropertyInfo propertyInfo)
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
        public virtual void SetValue(string propertyName, TProperty newValue)
        {
            SetValue(PropertyInfoFor(propertyName), newValue);
        }

        /// <summary>
        /// Set the backing value for the specified property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        /// <param name="newValue">object</param>
        public virtual void SetValue(PropertyInfo propertyInfo, TProperty newValue)
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
    }
}
