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
