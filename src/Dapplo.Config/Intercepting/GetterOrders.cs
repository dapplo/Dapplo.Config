// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This defines the order in which the setters are called
    /// </summary>
    public enum GetterOrders
    {
        /// <summary>
        /// This is the order for the getter which implements the ITransactionalProperties
        /// </summary>
        Transaction = 0,
        /// <summary>
        /// This is the order for the setter which places the value into the dictionary
        /// </summary>
        Dictionary = 2000
    }
}
