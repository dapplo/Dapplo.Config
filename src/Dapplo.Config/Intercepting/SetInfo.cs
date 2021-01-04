//  Dappl// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This provides the value for a set interceptor
    /// </summary>
    public class SetInfo<TProperty> : GetSetInfo
    {
        /// <summary>
        ///     Does property have an old value?
        /// </summary>
        public bool HasOldValue { get; set; }

        /// <summary>
        ///     The new value for the property
        /// </summary>
        public TProperty NewValue { get; set; }

        /// <summary>
        ///     The old value of the property, if any (see HasOldValue)
        /// </summary>
        public TProperty OldValue { get; set; }
    }
}
