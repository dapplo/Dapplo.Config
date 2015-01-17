﻿/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2014 Robin Krom
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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Dapplo.Config.Support;

namespace Dapplo.Config {
	/// <summary>
	///     Implementation of the PropertyProxy
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class PropertyProxy<T> : RealProxy, IPropertyProxy<T> {
		private readonly List<IPropertyProxyExtension<T>> _extensions = new List<IPropertyProxyExtension<T>>();
		private readonly List<Getter> _getters = new List<Getter>();
		private readonly IDictionary<string, List<Action<MethodCallInfo>>> _methodMap = new Dictionary<string, List<Action<MethodCallInfo>>>();
		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
		private readonly List<Setter> _setters = new List<Setter>();


		public PropertyProxy() : base(typeof (T)) {
			Type proxiedType = typeof (T);
			foreach (PropertyInfo propertyInfo in proxiedType.GetProperties()) {
				Attribute[] customAttributes = Attribute.GetCustomAttributes(propertyInfo);
				foreach (Attribute customAttribute in customAttributes) {
					if (customAttribute.GetType() == typeof (DefaultValueAttribute)) {
						var defaultValueAttribute = customAttribute as DefaultValueAttribute;
						if (defaultValueAttribute != null) {
							_properties[propertyInfo.Name] = defaultValueAttribute.Value;
						}
					}
				}
			}
			// Make sure the default set logic is registered
			RegisterSetter(0, DefaultSet);
			// Make sure the default get logic is registered
			RegisterGetter(0, DefaultGet);
		}

		/// <summary>
		///     Register a method for the proxy
		/// </summary>
		/// <param name="methodname"></param>
		/// <param name="methodAction"></param>
		/// <returns></returns>
		public IPropertyProxy<T> RegisterMethod(string methodname, Action<MethodCallInfo> methodAction) {
			List<Action<MethodCallInfo>> functions;
			if (!_methodMap.TryGetValue(methodname, out functions)) {
				functions = new List<Action<MethodCallInfo>>();
				_methodMap.Add(methodname, functions);
			}
			functions.Add(methodAction);
			return this;
		}

		/// <summary>
		///     Register a setter, this will be called when the proxy's set is called.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="setterAction"></param>
		/// <returns></returns>
		public IPropertyProxy<T> RegisterSetter(int order, Action<SetInfo> setterAction) {
			_setters.Add(new Setter {
				Order = order, SetterAction = setterAction
			});
			_setters.Sort();
			return this;
		}

		/// <summary>
		///     Register a getter, this will be called when the proxy's get is called.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="getterAction"></param>
		/// <returns></returns>
		public IPropertyProxy<T> RegisterGetter(int order, Action<GetInfo> getterAction) {
			_getters.Add(new Getter {
				Order = order, GetterAction = getterAction
			});
			_getters.Sort();
			return this;
		}

		public IPropertyProxy<T> AddExtension<TE>() where TE : IPropertyProxyExtension<T> {
			var extension = (TE) Activator.CreateInstance(typeof (TE), this);
			_extensions.Add(extension);
			return this;
		}

		public IPropertyProxy<T> AddExtension(Type extensionType) {
			var extension = (IPropertyProxyExtension<T>) Activator.CreateInstance(extensionType.MakeGenericType(typeof (T)), this);
			_extensions.Add(extension);
			return this;
		}

		public T PropertyObject {
			get {
				return (T) GetTransparentProxy();
			}
		}

		public IDictionary<string, object> Properties {
			get {
				return _properties;
			}
		}

		public IPropertyProxy<T> SetProperties(IDictionary<string, object> properties) {
			foreach (string propertyName in properties.Keys) {
				object propertyValue = properties[propertyName];
				if (_properties.ContainsKey(propertyName)) {
					_properties[propertyName] = propertyValue;
				} else {
					_properties.Add(propertyName, propertyValue);
				}
			}
			return this;
		}

		/// <summary>
		///     A default implementation of the set logic
		/// </summary>
		/// <param name="setInfo"></param>
		private void DefaultSet(SetInfo setInfo) {
			// Add the value to the dictionary
			if (_properties.ContainsKey(setInfo.PropertyName)) {
				_properties[setInfo.PropertyName] = setInfo.NewValue;
			} else {
				_properties.Add(setInfo.PropertyName, setInfo.NewValue);
			}
		}

		/// <summary>
		///     A default implementation of the get logic
		/// </summary>
		/// <param name="getInfo"></param>
		private void DefaultGet(GetInfo getInfo) {
			object value;
			if (getInfo.PropertyName != null && _properties.TryGetValue(getInfo.PropertyName, out value)) {
				getInfo.Value = value;
			}
		}

		/// <summary>
		///     Implementation of the Invoke for the RealProxy, this is the central logic which will call all the getters,setters
		///     etc.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public override IMessage Invoke(IMessage msg) {
			var methodCallMessage = msg as IMethodCallMessage;
			if (methodCallMessage == null) {
				return new ReturnMessage(null, null, 0, null, null);
			}
			// Get the parameters
			object[] parameters = methodCallMessage.InArgs;
			// Get the method name
			string methodName = methodCallMessage.MethodName;
			// Preparations for the property access
			string propertyName;
			if (methodName.StartsWith("get_")) {
				propertyName = methodName.Substring(4);
				var getInfo = new GetInfo {
					PropertyName = propertyName, CanContinue = true
				};
				foreach (Getter getter in _getters) {
					getter.GetterAction(getInfo);
					if (!getInfo.CanContinue || getInfo.Error != null) {
						break;
					}
				}
				if (getInfo.HasError) {
					return new ReturnMessage(getInfo.Error, methodCallMessage);
				}
				return new ReturnMessage(getInfo.Value, null, 0, null, methodCallMessage);
			}
			if (methodName.StartsWith("set_")) {
				propertyName = methodName.Substring(4);

				object oldValue;
				bool hasOldValue = _properties.TryGetValue(propertyName, out oldValue);
				var setInfo = new SetInfo {
					NewValue = parameters[0], PropertyName = propertyName, HasOldValue = hasOldValue, CanContinue = true, OldValue = oldValue
				};
				foreach (Setter setter in _setters) {
					setter.SetterAction(setInfo);
					if (!setInfo.CanContinue || setInfo.Error != null) {
						break;
					}
				}
				if (setInfo.HasError) {
					return new ReturnMessage(setInfo.Error, methodCallMessage);
				}
				return new ReturnMessage(null, null, 0, null, methodCallMessage);
			}

			List<Action<MethodCallInfo>> actions;
			if (_methodMap.TryGetValue(methodName, out actions)) {
				var methodCallInfo = new MethodCallInfo {
					MethodName = methodName, Arguments = parameters
				};
				foreach (var action in actions) {
					action(methodCallInfo);
				}
				if (methodCallInfo.HasError) {
					return new ReturnMessage(methodCallInfo.Error, methodCallMessage);
				}
				return new ReturnMessage(methodCallInfo.ReturnValue, null, 0, null, methodCallMessage);
			}
			return new ReturnMessage(new NotImplementedException("No implementation for " + methodName), methodCallMessage);
		}
	}
}