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

using System;
using System.Collections.Generic;
using Dapplo.Config.Support;
using System.Linq.Expressions;

namespace Dapplo.Config {
	/// <summary>
	/// Helper enum for the call order, used when registering the setter
	/// </summary>
	public enum CallOrder : int {
		First = int.MinValue,
		Middle = 0,
		Last = int.MaxValue
	}

	/// <summary>
	/// Base interface for the generic IPropertyProxy
	/// This can be used to store in Lists etc
	/// </summary>
	public interface IPropertyProxy {
		/// <summary>
		/// Direct access to the raw property values of the property object
		/// Can be used to modify the directly, or for load/save
		/// Assignment to this property will COPY all the supplied properties one-by-one. 
		/// It will keep the current values, that are not supplied.
		/// Call clean iF you want to make sure it only has the supplied values.
		/// </summary>
		IDictionary<string, object> Properties {
			get;
			set;
		}

		/// <summary>
		/// This can be used to access the property object
		/// via the non generic interface.
		/// </summary>
		object UntypedPropertyObject {
			get;
		}

		/// <summary>
		/// Return the type of the property object
		/// </summary>
		Type PropertyObjectType {
			get;
		}

		/// <summary>
		/// Register a method
		/// </summary>
		/// <param name="methodname">Name of the method to add</param>
		/// <param name="methodAction">Action to be called</param>
		void RegisterMethod(string methodname, Action<MethodCallInfo> methodAction);

		/// <summary>
		///     Register a setter, currently there is only the generic "catch all" setter
		/// </summary>
		/// <param name="order">int used for sorting, lower is before higher is after</param>
		/// <param name="setterAction">Function to be called</param>
		void RegisterSetter(int order, Action<SetInfo> setterAction);

		/// <summary>
		///     Register a getter, currently there is only the generic "catch all" getter
		/// </summary>
		/// <param name="order">int used for sorting, lower is before higher is after</param>
		/// <param name="getterAction">Function to be called</param>
		void RegisterGetter(int order, Action<GetInfo> getterAction);
	}

	/// <summary>
	/// The property proxy is implemented with this interface.
	/// </summary>
	/// <typeparam name="T">The type of the interface with the properties</typeparam>
	public interface IPropertyProxy<T> : IPropertyProxy {
		/// <summary>
		///     Get the proxy object which "implements" the interface.
		///     Use this to access the values.
		/// </summary>
		T PropertyObject {
			get;
		}
	}
}