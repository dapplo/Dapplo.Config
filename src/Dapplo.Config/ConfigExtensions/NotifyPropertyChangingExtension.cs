using System;
using System.ComponentModel;
using Dapplo.Config.Attributes;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.ConfigExtensions
{
    internal class NotifyPropertyChangingExtension<TProperty> : IConfigExtension<TProperty>, INotifyPropertyChanging
    {
        private readonly ConfigurationBase<TProperty> _parent;

        public NotifyPropertyChangingExtension(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public event PropertyChangingEventHandler PropertyChanging;

        /// <inheritdoc />
        public int GetOrder { get; } = -1;

        /// <inheritdoc />
        public int SetOrder { get; } = (int)SetterOrders.NotifyPropertyChanging;

        /// <summary>
        ///     This is the logic which is called to invoke the event.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="eventArgs">PropertyChangingEventArgs</param>
        private void InvokePropertyChanging(object sender, PropertyChangingEventArgs eventArgs)
        {
            PropertyChanging?.Invoke(sender, eventArgs);
        }

        /// <inheritdoc />
        public void Getter(GetInfo<TProperty> getInfo)
        {
            // This will not be called, as the getter order is negative
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This creates a NPC event if the values are changed
        /// </summary>
        /// <param name="setInfo">SetInfo with all the set call information</param>
        [GetSetInterceptor(SetterOrders.NotifyPropertyChanged, true)]
        public void Setter(SetInfo<TProperty> setInfo)
        {
            if (PropertyChanging is null)
            {
                return;
            }
            // Create the event if the property is changing
            if (setInfo.HasOldValue && Equals(setInfo.NewValue, setInfo.OldValue))
            {
                return;
            }
            var propertyChangingEventArgs = new PropertyChangingEventArgsEx(setInfo.PropertyInfo.Name, setInfo.OldValue, setInfo.NewValue);
            InvokePropertyChanging(Proxy, propertyChangingEventArgs);
        }
    }
}
