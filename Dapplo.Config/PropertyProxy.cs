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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Dapplo.Config.Support;
using System.Linq.Expressions;

namespace Dapplo.Config {
	/// <summary>
	///     Implementation of the PropertyProxy
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class PropertyProxy<T> : RealProxy, IPropertyProxy<T> {
		private readonly List<IPropertyProxyExtension<T>> _extensions = new List<IPropertyProxyExtension<T>>();
		private readonly List<Getter> _getters = new List<Getter>();
		private readonly IDictionary<string, List<Action<MethodCallInfo>>> _methodMap = new Dictionary<string, List<Action<MethodCallInfo>>>();
		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
		private readonly List<Setter> _setters = new List<Setter>();

		// Cache the GetTransparentProxy value, as it makes more sense
		private readonly T _transparentProxy;

		/// <summary>
		/// Constructor
		/// </summary>
		public PropertyProxy() : base(typeof (T)) {
			Type proxiedType = typeof (T);
			foreach (PropertyInfo propertyInfo in proxiedType.GetProperties()) {
				var defaultValueAttribute = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
				if (defaultValueAttribute != null) {
					_properties[propertyInfo.Name] = defaultValueAttribute.Value;
				}
			}
			// Register the GetType handler, use Lambda to make refactoring possible
			RegisterMethod(ConfigUtils.GetMemberName<object>(x => x.GetType()), HandleGetType);

			// Make sure the default set logic is registered
			RegisterSetter((int)CallOrder.Middle, DefaultSet);
			// Make sure the default get logic is registered
			RegisterGetter((int)CallOrder.Middle, DefaultGet);
			_transparentProxy = (T)GetTransparentProxy();
		}

		/// <summary>
		///     Register a method for the proxy
		/// </summary>
		/// <param name="methodname"></param>
		/// <param name="methodAction"></param>
		public void RegisterMethod(string methodname, Action<MethodCallInfo> methodAction) {
			List<Action<MethodCallInfo>> functions;
			if (!_methodMap.TryGetValue(methodname, out functions)) {
				functions = new List<Action<MethodCallInfo>>();
				_methodMap.Add(methodname, functions);
			}
			functions.Add(methodAction);
		}

		/// <summary>
		///     Register a setter, this will be called when the proxy's set is called.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="setterAction"></param>
		public void RegisterSetter(int order, Action<SetInfo> setterAction) {
			_setters.Add(new Setter {
				Order = order, SetterAction = setterAction
			});
			_setters.Sort();
		}

		/// <summary>
		///     Register a getter, this will be called when the proxy's get is called.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="getterAction"></param>
		public void RegisterGetter(int order, Action<GetInfo> getterAction) {
			_getters.Add(new Getter {
				Order = order, GetterAction = getterAction
			});
			_getters.Sort();
		}

		/// <summary>
		/// This is an implementation of the GetType which returns the interface
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void HandleGetType(MethodCallInfo methodCallInfo) {
			methodCallInfo.ReturnValue = typeof(T);
		}

		/// <summary>
		/// Add an extension to the proxy, these extensions contain logic which enhances the proxy
		/// </summary>
		/// <typeparam name="TE">Type of Extension</typeparam>
		public void AddExtension<TE>() where TE : IPropertyProxyExtension<T> {
			var extension = (TE) Activator.CreateInstance(typeof (TE), this);
			_extensions.Add(extension);
		}

		/// <summary>
		/// Add an extension to the proxy, these extensions contain logic which enhances the proxy
		/// </summary>
		/// <param name="extensionType">Type for the extension</param>
		public void AddExtension(Type extensionType) {
			var extension = (IPropertyProxyExtension<T>) Activator.CreateInstance(extensionType.MakeGenericType(typeof (T)), this);
			_extensions.Add(extension);
		}

		/// <summary>
		/// Get the property object which this Proxy maintains
		/// Without using the generic type
		/// </summary>
		public object UntypedPropertyObject {
			get {
				return _transparentProxy;
			}
		}

		/// <summary>
		/// Return the Type of the PropertyObject
		/// </summary>
		public Type PropertyObjectType {
			get {
				return typeof(T);
			}
		}

		/// <summary>
		/// Get the property object which this Proxy maintains
		/// </summary>
		public T PropertyObject {
			get {
				return _transparentProxy;
			}
		}

		/// <summary>
		/// Get the raw property values of the property object
		/// Can be used to modify the directly, or for load/save
		/// Assignment to this will copy all the supplied properties.
		/// </summary>
		public IDictionary<string, object> Properties {
			get {
				return _properties;
			}
			set {
				foreach (string key in value.Keys) {
					_properties.SafelyAddOrOverwrite(key, value[key]);
				}
			}
		}

		/// <summary>
		///     A default implementation of the set logic
		/// </summary>
		/// <param name="setInfo"></param>
		private void DefaultSet(SetInfo setInfo) {
			// Add the value to the dictionary
			_properties.SafelyAddOrOverwrite(setInfo.PropertyName, setInfo.NewValue);
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
		/// <returns>IMessage</returns>
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

		/// <summary>
		/// Return the default value for a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>default value object</returns>
		public object DefaultValue<TProp>(Expression<Func<T, TProp>> propertyExpression) {
			string propertyName = propertyExpression.GetMemberName();

			Type proxiedType = typeof(T);
			PropertyInfo propertyInfo = proxiedType.GetProperty(propertyName);
			return propertyInfo.GetDefaultValue();
		}

		/// <summary>
		/// Get the description attribute for a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>description</returns>
		public string Description<TProp>(Expression<Func<T, TProp>> propertyExpression) {
			string propertyName = propertyExpression.GetMemberName();

			Type proxiedType = typeof(T);
			PropertyInfo propertyInfo = proxiedType.GetProperty(propertyName);
			return propertyInfo.GetDescription();
		}
	}
}