﻿/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Config

	Dapplo.Config is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Config is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have Config a copy of the GNU Lesser General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.
 */

using System.ComponentModel;
using Dapplo.Config.Support;

namespace Dapplo.Config.Proxy.Implementation
{
	/// <summary>
	///     This class implements the NotifyPropertyChanging extension logic,
	///     which automatically generates NotifyPropertyChanging events when set is called.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (INotifyPropertyChanging))]
	internal class NotifyPropertyChangingExtension<T> : AbstractPropertyProxyExtension<T>
	{
		// Reference to the property object, which is supplied in the PropertyChanging event
		private readonly T _propertyObject;

		// The "backing" event
		private event PropertyChangingEventHandler PropertyChanging;

		public NotifyPropertyChangingExtension(IPropertyProxy<T> proxy) : base(proxy)
		{
			CheckType(typeof (INotifyPropertyChanging));

			_propertyObject = proxy.PropertyObject;
			proxy.RegisterMethod("add_PropertyChanging", AddPropertyChanging);
			proxy.RegisterMethod("remove_PropertyChanging", RemovePropertyChanging);
			// Register the NotifyPropertyChangingSetter as a last setter, it will call the NotifyPropertyChanging event
			proxy.RegisterSetter((int) CallOrder.Middle - 1, NotifyPropertyChangingSetter);
		}

		/// <summary>
		///     This creates a NPC event if the values are changing
		/// </summary>
		/// <param name="setInfo">SetInfo with all the set call information</param>
		private void NotifyPropertyChangingSetter(SetInfo setInfo)
		{
			if (PropertyChanging == null)
			{
				return;
			}
			// Create the event if the property is changing
			if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue))
			{
				var propertyChangingEventArgs = new PropertyChangingEventArgs(setInfo.PropertyName);
				if (DapploConfig.EventDispatcher != null && !DapploConfig.EventDispatcher.CheckAccess())
				{
					DapploConfig.EventDispatcher.BeginInvoke(PropertyChanging, this, propertyChangingEventArgs);
				}
				else
				{
					PropertyChanging(_propertyObject, propertyChangingEventArgs);
				}
			}
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanging event is registered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void AddPropertyChanging(MethodCallInfo methodCallInfo)
		{
			// Add the parameters which should contain the event handler
			PropertyChanging += (PropertyChangingEventHandler) methodCallInfo.Arguments[0];
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanging event is unregistered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RemovePropertyChanging(MethodCallInfo methodCallInfo)
		{
			// Remove the handler via the parameter which should contain the event handler
			PropertyChanging -= (PropertyChangingEventHandler) methodCallInfo.Arguments[0];
		}
	}
}