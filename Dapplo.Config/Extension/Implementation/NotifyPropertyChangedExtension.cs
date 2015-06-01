/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Support;
using System.ComponentModel;

namespace Dapplo.Config.Extension.Implementation
{
	/// <summary>
	///     This class implements the NotifyPropertyChanged extension logic,
	///     which automatically generates NotifyPropertyChanged events when set is called.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (INotifyPropertyChanged))]
	internal class NotifyPropertyChangedExtension<T> : AbstractPropertyProxyExtension<T>
	{
		// Reference to the property object, which is supplied in the PropertyChanged event
		private readonly T _propertyObject;

		// The "backing" event
		private event PropertyChangedEventHandler PropertyChanged;

		public NotifyPropertyChangedExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (INotifyPropertyChanged));
			_propertyObject = proxy.PropertyObject;
			proxy.RegisterMethod("add_PropertyChanged", AddPropertyChanged);
			proxy.RegisterMethod("remove_PropertyChanged", RemovePropertyChanged);
			// Register the NotifyPropertyChangedSetter as a last setter, it will call the NPC event
			proxy.RegisterSetter((int) CallOrder.Last, NotifyPropertyChangedSetter);
		}

		/// <summary>
		///     This creates a NPC event if the values are changed
		/// </summary>
		/// <param name="setInfo">SetInfo with all the set call information</param>
		private void NotifyPropertyChangedSetter(SetInfo setInfo)
		{
			if (PropertyChanged == null)
			{
				return;
			}
			// Create the event if the property changed
			if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
			{
				var propertyChangedEventArgs = new PropertyChangedEventArgs(setInfo.PropertyName);
				if (DapploConfig.EventDispatcher != null && !DapploConfig.EventDispatcher.CheckAccess())
				{
					DapploConfig.EventDispatcher.BeginInvoke(PropertyChanged, this, propertyChangedEventArgs);
				}
				else
				{
					PropertyChanged(_propertyObject, propertyChangedEventArgs);
				}
			}
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanged event is registered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void AddPropertyChanged(MethodCallInfo methodCallInfo)
		{
			// Add the parameters which should contain the event handler
			PropertyChanged += (PropertyChangedEventHandler) methodCallInfo.Arguments[0];
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanged event is unregistered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RemovePropertyChanged(MethodCallInfo methodCallInfo)
		{
			// Remove the handler via the parameter which should contain the event handler
			PropertyChanged -= (PropertyChangedEventHandler) methodCallInfo.Arguments[0];
		}
	}
}