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
using System.Reflection;
using System.Linq;
using Dapplo.Config.Attributes;
using Dapplo.Utils.Extensions;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This is the type information for configuration types.
    /// </summary>
    public class SetGetInterceptInformation
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
        /// Fill all the values
        /// </summary>
        /// <param name="containingType">Type</param>
        public SetGetInterceptInformation(Type containingType)
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

        }
    }
}
