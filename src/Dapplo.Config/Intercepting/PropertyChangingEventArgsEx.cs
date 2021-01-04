// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Dapplo.Config.Intercepting
{
    /// <summary>
    /// This extends the PropertyChangingEventArgs with additional information
    /// </summary>
    public class PropertyChangingEventArgsEx : PropertyChangingEventArgs
    {
        /// <summary>
        /// The value before the change which is coming
        /// </summary>
        public object OldValue { get; }
        /// <summary>
        /// The value when the change is applied
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="oldValue">object</param>
        /// <param name="newValue">object</param>
        public PropertyChangingEventArgsEx(string propertyName, object oldValue, object newValue) : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
