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
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Config;
using Dapplo.Log;
using Dapplo.Utils.Extensions;
using Microsoft.Win32;
using AutoProperties;

namespace Dapplo.Registry
{
    /// <summary>
    /// This implements a window into the registry based on an interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RegistryBase<T> : ConfigurationBase<T>, IRegistry
    {
        private readonly RegistryAttribute _registryAttribute = typeof(T).GetAttribute<RegistryAttribute>() ?? new RegistryAttribute();
        private readonly IDictionary<string, RegistryAttribute> _registryAttributes = new Dictionary<string, RegistryAttribute>();

        #region Needed as workaround for a bug in Autoproperties

        /// <inheritdoc />
        [GetInterceptor]
        protected override object Getter(string propertyName)
        {
            return base.Getter(propertyName);
        }

        /// <inheritdoc />
        [SetInterceptor]
        protected override void Setter(string propertyName, object newValue)
        {
            base.Setter(propertyName, newValue);
        }
        #endregion

        /// <inheritdoc />
        protected override void OneTimePropertyInitializer(PropertyInfo propertyInfo)
        {
            base.OneTimePropertyInitializer(propertyInfo);

            var registryAttribute = propertyInfo.GetAttribute<RegistryAttribute>();
            if (registryAttribute == null)
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
        /// Initialize the property
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo</param>
        protected override void PropertyInitializer(PropertyInfo propertyInfo)
        {
            base.PropertyInitializer(propertyInfo);

            if (!_registryAttributes.TryGetValue(propertyInfo.Name, out var registryPropertyAttribute) || registryPropertyAttribute == null)
            {
                throw new ArgumentException($"{propertyInfo.Name} isn't correctly configured");
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
                        if (key == null)
                        {
                            throw new ArgumentException($"No registry entry in {hive}/{path} for {view}");
                        }

                        if (registryPropertyAttribute.ValueName == null)
                        {
                            // Read all values, assume IDictionary<string, object>
                            IDictionary<string, object> values;
                            var getInfo = GetValue(propertyInfo.Name);
                            if (!getInfo.HasValue)
                            {
                                // No value yet, create a new default
                                values = new SortedDictionary<string, object>();
                                Setter(propertyInfo, values);
                            }
                            else
                            {
                                values = (IDictionary<string, object>)getInfo.Value;
                            }

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
                        }
                        else
                        {
                            // Read a specific value
                            Setter(propertyInfo, key.GetValue(registryPropertyAttribute.ValueName));
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

        /// <summary>
        ///     The path in the registry for a property
        /// </summary>
        /// <param name="propertyExpression">expression to identify the property</param>
        /// <returns>string</returns>
        public string PathFor<TProp>(Expression<Func<T, TProp>> propertyExpression)
        {
            return PathFor(propertyExpression.GetMemberName());
        }
    }
}