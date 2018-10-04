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
using System.Linq;
using System.Reflection;
using Dapplo.Log;
using Dapplo.Utils;

namespace Dapplo.Config
{
    /// <summary>
    /// An abstract non generic ConfigurationBase.
    /// This defines the API for the configuration based implementations
    /// </summary>
    public abstract class ConfigurationBase
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
        /// Initialize the whole thing, this should be called from the final class
        /// </summary>
        /// <param name="typeToInitializeFor">Type to analyze the properties on</param>
        protected void Initialize(Type typeToInitializeFor)
        {
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
    }
}
