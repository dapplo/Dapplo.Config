using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Config;
using Dapplo.Log;
using Dapplo.Utils.Extensions;
using Microsoft.Win32;

namespace Dapplo.Registry
{
    /// <summary>
    /// This implements a window into the registry based on an interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RegistryBase<T> : ConfigurationBase<T>, IRegistry
    {
        private readonly RegistryAttribute _registryAttribute;
        private readonly IDictionary<string, RegistryPropertyAttribute> _registryAttributes = new Dictionary<string, RegistryPropertyAttribute>();

        /// <summary>
        /// Constructor
        /// </summary>
        public RegistryBase()
        {
            _registryAttribute = typeof(T).GetAttribute<RegistryAttribute>() ?? new RegistryAttribute();
        }

        /// <inheritdoc />
        protected override void OneTimePropertyInitializer(PropertyInfo propertyInfo)
        {
            base.OneTimePropertyInitializer(propertyInfo);

            var registryPropertyAttribute = propertyInfo.GetAttribute<RegistryPropertyAttribute>();
            if (registryPropertyAttribute == null)
            {
                throw new ArgumentException($"{propertyInfo.Name} doesn't have a path mapping");
            }

            var path = registryPropertyAttribute.Path;
            if (_registryAttribute.Path != null)
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

            registryPropertyAttribute.Path = path;

            // Store for retrieval
            _registryAttributes[propertyInfo.Name] = registryPropertyAttribute;
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
                var path = _registryAttribute.Path;
                using (var key = baseKey.OpenSubKey(path))
                {
                    try
                    {
                        if (key == null)
                        {
                            throw new ArgumentException($"No registry entry in {hive}/{path} for {view}");
                        }

                        if (registryPropertyAttribute.Value == null)
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
                            Setter(propertyInfo, key.GetValue(registryPropertyAttribute.Value));
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