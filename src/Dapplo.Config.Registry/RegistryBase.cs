﻿// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Dapplo.Log;
using Microsoft.Win32;
using Dapplo.Config.Attributes;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Extensions;

namespace Dapplo.Config.Registry
{
    /// <summary>
    /// This implements a window into the registry based on an interface
    /// </summary>
    /// <typeparam name="TInterface">Interface for the registry values</typeparam>
    public class RegistryBase<TInterface> : ConfigurationBase<object>, IRegistry
    {
        private readonly RegistryAttribute _registryAttribute = typeof(TInterface).GetAttribute<RegistryAttribute>() ?? new RegistryAttribute();
        private readonly IDictionary<string, RegistryAttribute> _registryAttributes = new Dictionary<string, RegistryAttribute>();

        // TODO: Add registry monitoring from Dapplo.Windows.Advapi32
        // RegistryMonitor.ObserveChanges(RegistryHive.LocalMachine, subkey)

        /// <summary>
        /// Factory for IniSectionBase implementations
        /// </summary>
        /// <returns>TInterface</returns>
        public static TInterface Create()
        {
            return ConfigProxy.Create<TInterface>(new RegistryBase<TInterface>());
        }

        /// <inheritdoc />
        protected RegistryBase()
        {
            Initialize(typeof(TInterface));
        }

        /// <summary>
        /// Retrieves the value from the registry
        /// </summary>
        /// <param name="getInfo">GetInfo</param>
        [GetSetInterceptor(GetterOrders.Dictionary)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void FromRegistryGetter(GetInfo<object> getInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var propertyName = getInfo.PropertyInfo.Name;
            if (!_registryAttributes.TryGetValue(propertyName, out var registryPropertyAttribute) || registryPropertyAttribute is null)
            {
                throw new ArgumentException($"{propertyName} isn't correctly configured");
            }

            var hive = _registryAttribute.Hive;
            if (registryPropertyAttribute.HasHive)
            {
                hive = registryPropertyAttribute.Hive;
            }

            var view = _registryAttribute.View;
            if (registryPropertyAttribute.HasView)
            {
                view = registryPropertyAttribute.View;
            }

            using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
            {
                var path = registryPropertyAttribute.Path;
                using (var key = baseKey.OpenSubKey(path))
                {
                    if (key is null)
                    {
                        throw new ArgumentException($"No registry entry in {hive}/{path} for {view}");
                    }

                    // TODO: Convert the returned value to the correct property type
                    if (registryPropertyAttribute.ValueName is null)
                    {
                        // Read all values, assume IDictionary<string, object>
                        IDictionary<string, object> values = new SortedDictionary<string, object>();
                        foreach (var valueName in key.GetValueNames())
                        {
                            var value = key.GetValue(valueName);
                            if (!values.ContainsKey(valueName))
                            {
                                values.Add(valueName, value);
                            }
                            else
                            {
                                values[valueName] = value;
                            }
                        }
                        getInfo.Value = values;
                        getInfo.HasValue = true;
                        return;
                    }
                    getInfo.Value = key.GetValue(registryPropertyAttribute.ValueName);
                    getInfo.HasValue = true;
                }
            }
        }

        /// <summary>
        /// Retrieves the value from the dictionary
        /// </summary>
        /// <param name="setInfo">GetInfo</param>
        [GetSetInterceptor(SetterOrders.Dictionary, true)]
        // ReSharper disable once UnusedMember.Local as this is processed via reflection
#pragma warning disable IDE0051 // Remove unused private members
        private void ToDictionarySetter(SetInfo<object> setInfo)
#pragma warning restore IDE0051 // Remove unused private members
        {
            var propertyName = setInfo.PropertyInfo.Name;
            var newValue = setInfo.NewValue;
            if (!_registryAttributes.TryGetValue(propertyName, out var registryPropertyAttribute) || registryPropertyAttribute is null)
            {
                throw new ArgumentException($"{propertyName} isn't correctly configured");
            }

            var hive = _registryAttribute.Hive;
            if (registryPropertyAttribute.HasHive)
            {
                hive = registryPropertyAttribute.Hive;
            }

            var view = _registryAttribute.View;
            if (registryPropertyAttribute.HasView)
            {
                view = registryPropertyAttribute.View;
            }

            using (var baseKey = RegistryKey.OpenBaseKey(hive, view))
            {
                var path = registryPropertyAttribute.Path;
                using (var key = baseKey.OpenSubKey(path))
                {
                    try
                    {
                        if (key is null)
                        {
                            throw new ArgumentException($"No registry entry in {hive}/{path} for {view}");
                        }
                        // TODO: Convert the  property type to the correct registry value
                        if (registryPropertyAttribute.ValueName is null)
                        {
                            if (!(newValue is IDictionary<string, object> newValues))
                            {
                                return;
                            }

                            foreach (var valueName in newValues.Keys)
                            {
                                key.SetValue(valueName, newValues[valueName]);
                            }
                        }
                        else
                        {
                            key.SetValue(registryPropertyAttribute.ValueName, newValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn().WriteLine(ex.Message);
                        if (!registryPropertyAttribute.IgnoreErrors)
                        {
                            throw;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void PropertyInitializer(PropertyInfo propertyInfo)
        {
            base.PropertyInitializer(propertyInfo);

            var registryAttribute = propertyInfo.GetAttribute<RegistryAttribute>();
            if (registryAttribute is null)
            {
                throw new ArgumentException($"{propertyInfo.Name} doesn't have a path mapping");
            }

            var path = registryAttribute.Path;
            if (_registryAttribute.Path != null && !registryAttribute.IgnoreBasePath)
            {
                path = Path.Combine(_registryAttribute.Path, path);
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException($"{propertyInfo.Name} doesn't have a path mapping");
            }

            if (path.StartsWith(@"\"))
            {
                path = path.Remove(0, 1);
            }

            registryAttribute.Path = path;

            // Store for retrieval
            _registryAttributes[propertyInfo.Name] = registryAttribute;
        }

        /// <summary>
        ///     The path in the registry for a property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>string</returns>
        public string PathFor(string propertyName)
        {
            if (_registryAttributes.TryGetValue(propertyName, out var registryPropertyAttribute))
            {
                return registryPropertyAttribute.Path;
            }
            return null;
        }
    }
}