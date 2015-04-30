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

namespace Dapplo.Config.Extension.Implementation {
	/// <summary>
	///     This class implements the NotifyPropertyChanging extension logic,
	///     which automatically generates NotifyPropertyChanging events when set is called.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[Extension(typeof (INotifyPropertyChanging))]
	internal class NotifyPropertyChangingExtension<T> : AbstractPropertyProxyExtension<T> {
		// Reference to the property object, which is supplied in the PropertyChanging event
		private readonly T _propertyObject;

		// The "backing" event
		private event PropertyChangingEventHandler PropertyChanging;

		public NotifyPropertyChangingExtension(IPropertyProxy<T> proxy) : base(proxy) {
			CheckType(typeof (INotifyPropertyChanging));

			_propertyObject = proxy.PropertyObject;
			proxy.RegisterMethod("add_PropertyChanging", AddPropertyChanging);
			proxy.RegisterMethod("remove_PropertyChanging", RemovePropertyChanging);
			// Register the NotifyPropertyChangingSetter as a last setter, it will call the NotifyPropertyChanging event
			proxy.RegisterSetter((int)CallOrder.Middle - 1, NotifyPropertyChangingSetter);
		}

		/// <summary>
		///     This creates a NPC event if the values are changing
		/// </summary>
		/// <param name="setInfo">SetInfo with all the set call information</param>
		private void NotifyPropertyChangingSetter(SetInfo setInfo) {
			if (PropertyChanging == null) {
				return;
			}
			// Create the event if the property is changing
			if (!setInfo.HasOldValue || !Equals(setInfo.NewValue, setInfo.OldValue)) {
				PropertyChanging(_propertyObject, new PropertyChangingEventArgs(setInfo.PropertyName));
			}
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanging event is registered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void AddPropertyChanging(MethodCallInfo methodCallInfo) {
			// Add the parameters which should contain the event handler
			PropertyChanging += (PropertyChangingEventHandler) methodCallInfo.Arguments[0];
		}

		/// <summary>
		///     This is the logic which is called when the PropertyChanging event is unregistered.
		/// </summary>
		/// <param name="methodCallInfo">MethodCallInfo</param>
		private void RemovePropertyChanging(MethodCallInfo methodCallInfo) {
			// Remove the handler via the parameter which should contain the event handler
			PropertyChanging -= (PropertyChangingEventHandler)methodCallInfo.Arguments[0];
		}
	}
}