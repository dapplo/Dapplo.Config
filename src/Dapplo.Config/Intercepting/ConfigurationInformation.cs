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
using System.Reflection;
using System.Linq;
using Dapplo.Config.Attributes;
using Dapplo.Utils.Extensions;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This is the type information for configuration types.
    /// </summary>
    public class ConfigurationInformation
    {
        /// <summary>
        /// Store of setter methods
        /// </summary>
        public MethodInfo[] SetterMethods { get; }

        /// <summary>
        /// Store of setter methods
        /// </summary>
        public MethodInfo[] GetterMethods { get; }

        /// <summary>
        /// Store of PropertyInfos for every property
        /// </summary>
        public IReadOnlyDictionary<string, PropertyInfo> PropertyInfos { get; }

        /// <summary>
        /// Fill all the values
        /// </summary>
        /// <param name="containingType"></param>
        /// <param name="intefaceType"></param>
        public ConfigurationInformation(Type containingType, Type intefaceType)
        {
            var methods = containingType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            SetterMethods = methods
                .Select(methodInfo => new Tuple<MethodInfo, SetterAttribute>(methodInfo, methodInfo.GetAttribute<SetterAttribute>()))
                .Where(tuple => tuple.Item2 != null)
                .OrderBy(tuple => tuple.Item2.Order)
                .Select(tuple => tuple.Item1).ToArray();
            GetterMethods = methods
                .Select(methodInfo => new Tuple<MethodInfo, GetterAttribute>(methodInfo, methodInfo.GetAttribute<GetterAttribute>()))
                .Where(tuple => tuple.Item2 != null)
                .OrderBy(tuple => tuple.Item2.Order)
                .Select(tuple => tuple.Item1).ToArray();

            var typeToAnalyze = intefaceType ?? containingType;
            var types = new[] { typeToAnalyze }.Concat(typeToAnalyze.GetInterfaces());

            var propertyInfos = new Dictionary<string, PropertyInfo>();
            foreach (var type in types)
            {
                foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var propertyName = propertyInfo.Name;
                    if (propertyInfos.ContainsKey(propertyName))
                    {
                        continue;
                    }
                    if ("Item".Equals(propertyName))
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
