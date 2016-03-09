//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Config
// 
//  Dapplo.Config is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Config is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System.ComponentModel;
using Dapplo.Config.Interceptor;

#endregion

namespace Dapplo.Config.Proxy.Implementation
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
		///     This is the logic which is called when the PropertyChanged event is registered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void AddPropertyChanged(MethodCallInfo methodCallInfo)
		{
			// Add the parameters which should contain the event handler
			PropertyChanged += (PropertyChangedEventHandler) methodCallInfo.Arguments[0];
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

		// The "backing" event
		private event PropertyChangedEventHandler PropertyChanged;

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