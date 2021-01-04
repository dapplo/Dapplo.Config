// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.Extensions
{
    /// <summary>
    ///     Configuration extensions
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// A convenience extension to get the target typed. 
        /// </summary>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="configuration">IConfiguration</param>
        /// <returns>TConfig</returns>
        public static TTarget Target<TTarget>(this IConfiguration configuration)
            where TTarget : ConfigurationBase
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (configuration is ConfigProxy configProxy)
            {
                return configProxy.Target as TTarget;
            }

            return null;
        }
    }
}