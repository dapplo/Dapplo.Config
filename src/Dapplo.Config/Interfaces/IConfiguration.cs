// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    /// Marker interface
    /// </summary>
    public interface IConfiguration
    {

    }

    /// <summary>
    /// The base interface for configuration classes
    /// </summary>
    public interface IConfiguration<TProperty> : IConfiguration, IDescription, IWriteProtectProperties, IHasChanges, IDefaultValue, INotifyPropertyChanged, INotifyPropertyChanging, ITransactionalProperties, ITagging, IShallowCloneable
    {
        /// <summary>
        /// Return all properties with their current value
        /// </summary>
        IEnumerable<KeyValuePair<string, TProperty>> Properties();

        /// <summary>
        /// Returns all the property names
        /// </summary>
        /// <returns>IEnumerable with string</returns>
        IEnumerable<string> PropertyNames();
    }
}
