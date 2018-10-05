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
using System.Collections.Generic;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This is the type information for configuration types.
    /// </summary>
    public class GetSetInterceptInformation
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
        public GetSetInterceptInformation(Type containingType)
        {
            var setterMethods = new List<MethodInfo>();
            var getterMethods = new List<MethodInfo>();
            var currentType = containingType;
            do
            {
                foreach(var methodInfo in currentType.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (methodInfo.Name.Length < 7 && !methodInfo.Name.EndsWith("etter"))
                    {
                        continue;
                    }
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length != 1)
                    {
                        continue;
                    }
                    var parameterType = parameters[0].ParameterType;
                    if (parameterType != typeof(GetInfo) && parameterType != typeof(SetInfo))
                    {
                        continue;
                    }
                    var gOrS = methodInfo.Name[methodInfo.Name.Length - 6];
                    if (gOrS == 'G')
                    {
                        getterMethods.Add(methodInfo);
                    }
                    else if(gOrS == 'S')
                    {
                        setterMethods.Add(methodInfo);
                    }
                }
                currentType = currentType.BaseType;
            } while (currentType != typeof(object));

            GetterMethods = getterMethods
                .OrderBy(methodInfo => methodInfo.GetAttribute<InterceptOrderAttribute>().Order)
                .ToArray();
            SetterMethods = setterMethods
                .OrderBy(methodInfo => methodInfo.GetAttribute<InterceptOrderAttribute>().Order)
                .ToArray();
        }
    }
}
