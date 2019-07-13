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