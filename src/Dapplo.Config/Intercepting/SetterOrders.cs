// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This defines the order in which the setters are called
    /// </summary>
    public enum SetterOrders
    {
        /// <summary>
        /// This is the order for the setter which implements the IWriteProtectProperties
        /// </summary>
        WriteProtect = int.MinValue,
        /// <summary>
        /// This is the order for the setter which implements the INotifyPropertyChanging
        /// </summary>
        SetInfoInitializer = 0,
        /// <summary>
        /// This is the order for the setter which implements the INotifyPropertyChanging
        /// </summary>
        NotifyPropertyChanging = 1000,
        /// <summary>
        /// This is the order for the setter which implements the ITransactionalProperties
        /// </summary>
        Transaction = 2000,
        /// <summary>
        /// This is the order for the setter which implements the IHasChanges
        /// </summary>
        HasChanges = 3000,
        /// <summary>
        /// This is the order for the setter which places the value into the dictionary
        /// </summary>
        Dictionary = 4000,
        /// <summary>
        /// This is the order for the setter which implements the INotifyPropertyChanged
        /// </summary>
        NotifyPropertyChanged = 5000
    }
}
