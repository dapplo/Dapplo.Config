using System.Collections.Generic;
using System.ComponentModel;
using Dapplo.Utils;

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    /// The base interface for configuration classes
    /// </summary>
    public interface IConfiguration : IDescription, IWriteProtectProperties, IHasChanges, IDefaultValue, INotifyPropertyChanged, INotifyPropertyChanging, ITransactionalProperties, ITagging, IShallowCloneable
    {
        /// <summary>
        /// Return all properties with their current value
        /// </summary>
        IEnumerable<KeyValuePair<string, object>> Properties { get;  }

        /// <summary>
        /// (Re)Initialize this class with new properties
        /// </summary>
        /// <param name="properties">IDictionary with the properties, or null for empty</param>
        void Initialize(IDictionary<string, object> properties = null);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IConfiguration<T> : IConfiguration, IDescription<T>,
        IWriteProtectProperties<T>,
        IHasChanges<T>,
        IDefaultValue<T>,
        ITagging<T>
    {
    }
}
