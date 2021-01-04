// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This is the type information for configuration types.
    /// </summary>
    public class PropertiesInformation
    {
        /// <summary>
        /// Store of PropertyInfos for every property
        /// </summary>
        public IReadOnlyDictionary<string, PropertyInfo> PropertyInfos { get; }

        /// <summary>
        /// Fill all the values
        /// </summary>
        /// <param name="interfaceType">Type</param>
        public PropertiesInformation(Type interfaceType)
        {
            var types = new[] { interfaceType }.Concat(interfaceType.GetInterfaces());

            var propertyInfos = new Dictionary<string, PropertyInfo>(AbcComparer.Instance);
            foreach (var type in types)
            {
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var propertyName = propertyInfo.Name;
                    // We already have the property, skip it
                    if (propertyInfos.ContainsKey(propertyName))
                    {
                        continue;
                    }
                    // Skip indexer
                    if (propertyInfo.GetIndexParameters().Any())
                    {
                        continue;
                    }

                    propertyInfos[propertyName] = propertyInfo;
                }
            }
            PropertyInfos = propertyInfos;
        }

        /// <summary>
        /// Helper method to find the PropertyInfo
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>PropertyInfo</returns>
        public PropertyInfo PropertyInfoFor(string propertyName)
        {
            if (!PropertyInfos.TryGetValue(propertyName, out var propertyInfo))
            {
                throw new NotSupportedException($"Property {propertyName} doesn't exist.");
            }

            return propertyInfo;
        }
    }
}
