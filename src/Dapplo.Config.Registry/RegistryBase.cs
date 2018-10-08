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
using System.IO;
using System.Reflection;
using Dapplo.Log;
using Dapplo.Utils.Extensions;
using Microsoft.Win32;
using Dapplo.Config.Attributes;
using Dapplo.Config.Intercepting;

namespace Dapplo.Config.Registry
{
    /// <summary>
    /// This implements a window into the registry based on an interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RegistryBase<T> : ConfigurationBase, IRegistry
    {
        private readonly RegistryAttribute _registryAttribute = typeof(T).GetAttribute<RegistryAttribute>() ?? new RegistryAttribute();
        private readonly IDictionary<string, RegistryAttribute> _registryAttributes = new Dictionary<string, RegistryAttribute>();

        // TODO: Add registry monitoring from Dapplo.Windows.Advapi32
        // RegistryMonitor.ObserveChanges(RegistryHive.LocalMachine, subkey)

        /// <inheritdoc />
        public RegistryBase()
        {
            Initialize(typeof(T));
        }

        /// <summary>
        /// Retrieves the value from the registry
        /// </summary>
        /// <param name="getInfo">GetInfo</param>
        [InterceptOrder(GetterOrders.Dictionary)]
        private void FromRegistryGetter(GetInfo getInfo)
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
        [InterceptOrder(SetterOrders.Dictionary)]
        private void ToDictionarySetter(SetInfo setInfo)
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