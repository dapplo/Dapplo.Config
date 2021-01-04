// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reflection;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// Information for a Get or Set invocation
    /// </summary>
    public class GetSetInfo
    {
        /// <summary>
        ///     Can the proxy continue with other getter/setters?
        ///     This should be set to false if a getter/setter implementation wants to throw an exception or thinks there should be
        ///     no more others.
        /// </summary>
        public bool CanContinue { get; set; } = true;

        /// <summary>
        ///    PropertyInfo of the property that is being get/set
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }
}
