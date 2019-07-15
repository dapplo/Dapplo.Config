using System;
using System.ComponentModel;
using Dapplo.Config.Attributes;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.ConfigExtensions
{
    internal class NotifyPropertyChangedExtension<TProperty> : IConfigExtension<TProperty>, INotifyPropertyChanged
    {
        private readonly ConfigurationBase<TProperty> _parent;

        public NotifyPropertyChangedExtension(ConfigurationBase<TProperty> parent)
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
        public int GetOrder { get; } = -1;

        /// <inheritdoc />
        public int SetOrder { get; } = (int)SetterOrders.NotifyPropertyChanged;

        /// <summary>
        ///     This is the logic which is called to invoke the event.
        /// </summary>
        /// <param name="sender">object</param>
        /// <param name="eventArgs">PropertyChangedEventArgs</param>
        private void InvokePropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(sender, eventArgs);
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
            // Fast exit when no listeners.
            if (PropertyChanged is null)
            {
                return;
            }
            // Create the event if the property changed
            if (setInfo.HasOldValue && Equals(setInfo.NewValue, setInfo.OldValue))
            {
                return;
            }
            var propertyChangedEventArgs = new PropertyChangedEventArgsEx(setInfo.PropertyInfo.Name, setInfo.OldValue, setInfo.NewValue);
            InvokePropertyChanged(Proxy, propertyChangedEventArgs);
        }
    }
}
