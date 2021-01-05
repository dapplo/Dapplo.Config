// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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

        /// <summary>
        /// This is the target for the proxy
        /// </summary>
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
                // Make the target available to the proxy, 
                configProxy.Target = target;
                // Make the proxy available to the target, used for NotifyPropertyChang ed/ing events
                target.Proxy = configProxy;
            }

            return proxy;
        }
    }
}
