// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This provides the value for a get interceptor
    /// </summary>
    public class GetInfo<TProperty> : GetSetInfo
    {
        /// <summary>
        ///     The value of the property
        /// </summary>
        public bool HasValue { get; set; }

        /// <summary>
        ///     The value of the property
        /// </summary>
        public TProperty Value { get; set; }
    }
}
