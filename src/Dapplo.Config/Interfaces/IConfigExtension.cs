using Dapplo.Config.Intercepting;

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    /// This interface defines an extension for the configuration framework
    /// </summary>
    /// <typeparam name="TProperty">Type of the property to store</typeparam>
    public interface IConfigExtension<TProperty>
    {
        /// <summary>
        /// Returns the order for the getter
        /// </summary>
        int GetOrder { get; }

        /// <summary>
        /// Returns the order for the setter
        /// </summary>
        int SetOrder { get; }

        /// <summary>
        /// The getter
        /// </summary>
        /// <param name="getInfo">GetInfo with all the information for the getter</param>
        void Getter(GetInfo<TProperty> getInfo);

        /// <summary>
        /// The setter
        /// </summary>
        /// <param name="setInfo">SetInfo with all the information for the setter</param>
        void Setter(SetInfo<TProperty> setInfo);
    }
}
