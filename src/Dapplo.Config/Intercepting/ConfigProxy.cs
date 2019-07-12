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
using System.Linq;
using System.Reflection;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This wraps interface calls to a intercepted implementation
    /// </summary>
    public class ConfigProxy : DispatchProxy
    {
        private readonly Dictionary<MethodInfo, MethodInfo> _methodCache = new Dictionary<MethodInfo, MethodInfo>();
        public ConfigurationBase Target { get; private set; }

        /// <summary>
        /// Implement invoke of the DispatchProxy
        /// </summary>
        /// <param name="targetMethod">MethodInfo</param>
        /// <param name="args">object array</param>
        /// <returns>object</returns>
        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            if (targetMethod.Name == "get_Item" && args[0] is string index)
            {
                return Target.Getter(index);
            }

            if (targetMethod.Name.StartsWith("get_"))
            {
                return Target.Getter(targetMethod.Name.Substring(4));
            }
            if (targetMethod.Name.StartsWith("set_"))
            {
                Target.Setter(targetMethod.Name.Substring(4), args[0]);
                return null;
            }

            if (!_methodCache.TryGetValue(targetMethod, out var proxiedTargetMethod))
            {
                proxiedTargetMethod = Target.GetType().GetMethod(targetMethod.Name, targetMethod.GetParameters().Select(p => p.ParameterType).ToArray());
                _methodCache[targetMethod] = proxiedTargetMethod;
            }

            if (proxiedTargetMethod == null)
            {
                throw new MissingMethodException(targetMethod.ToString());
            }
            return proxiedTargetMethod.Invoke(Target, args);
        }

        /// <summary>
        /// Factory for ConfigProxy object
        /// </summary>
        /// <typeparam name="TInterface">type of the interface to implement</typeparam>
        /// <param name="target">ConfigurationBase</param>
        /// <returns></returns>
        public static TInterface Create<TInterface>(ConfigurationBase target)
        {
            var proxy = DispatchProxy.Create<TInterface, ConfigProxy>();
            if (proxy is ConfigProxy configProxy)
            {
                configProxy.Target = target;
            }

            return proxy;
        }
    }
}
