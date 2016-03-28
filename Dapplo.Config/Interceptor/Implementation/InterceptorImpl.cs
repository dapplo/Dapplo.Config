﻿//  Dapplo - building blocks for desktop applications
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Config.Support;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.Config.Interceptor.Implementation
{
	/// <summary>
	///     Implementation of the IInterceptor
	/// </summary>
	internal sealed class InterceptorImpl<TInterceptor> : IInterceptor
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly LogSource Log = new LogSource();
		private readonly List<IInterceptorExtension> _extensions = new List<IInterceptorExtension>();
		private readonly List<Getter> _getters = new List<Getter>();
		private readonly IDictionary<string, List<Action<MethodCallInfo>>> _methodMap = new Dictionary<string, List<Action<MethodCallInfo>>>();
		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>(AbcComparer.Instance);
		private readonly List<Setter> _setters = new List<Setter>();

		/// <summary>
		///     Cache the AllPropertyInfos
		/// </summary>
		private IDictionary<string, PropertyInfo> _allPropertyInfos;

		/// <summary>
		///     Constructor
		/// </summary>
		public InterceptorImpl()
		{
			// Register the GetType handler, use Lambda to make refactoring possible
			// ReSharper disable ReturnValueOfPureMethodIsNotUsed
			RegisterMethod(ExpressionExtensions.GetMemberName<object>(x => x.GetType()), HandleGetType);
			RegisterMethod(ExpressionExtensions.GetMemberName<object>(x => x.GetHashCode()), HandleGetHashCode);
			RegisterMethod(ExpressionExtensions.GetMemberName<object>(x => x.Equals(null)), HandleEquals);
			RegisterMethod(ExpressionExtensions.GetMemberName<object>(x => x.Equals(null)), HandleEquals);
			// ReSharper restore ReturnValueOfPureMethodIsNotUsed
			// Make sure the default set logic is registered
			RegisterSetter((int) CallOrder.Middle, DefaultSet);
			// Make sure the default get logic is registered
			RegisterGetter((int) CallOrder.Middle, DefaultGet);
		}

		/// <summary>
		///     Get the Type for a property
		/// </summary>
		public IDictionary<string, Type> PropertyTypes { get; } = new Dictionary<string, Type>(AbcComparer.Instance);

		/// <summary>
		///     Register a method for the proxy
		/// </summary>
		/// <param name="methodname"></param>
		/// <param name="methodAction"></param>
		public void RegisterMethod(string methodname, Action<MethodCallInfo> methodAction)
		{
			Log.Verbose().WriteLine("Registering method {0}", methodname);
			List<Action<MethodCallInfo>> functions;
			if (!_methodMap.TryGetValue(methodname, out functions))
			{
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
		public void RegisterSetter(int order, Action<SetInfo> setterAction)
		{
			_setters.Add(new Setter
			{
				Order = order,
				SetterAction = setterAction
			});
			_setters.Sort();
		}

		/// <summary>
		///     Register a getter, this will be called when the proxy's get is called.
		/// </summary>
		/// <param name="order"></param>
		/// <param name="getterAction"></param>
		public void RegisterGetter(int order, Action<GetInfo> getterAction)
		{
			_getters.Add(new Getter
			{
				Order = order,
				GetterAction = getterAction
			});
			_getters.Sort();
		}

		/// <summary>
		///     Simple getter for all properties on the type, including derrived interfaces
		/// </summary>
		public IDictionary<string, PropertyInfo> AllPropertyInfos
		{
			get
			{
				if (_allPropertyInfos == null)
				{
					// Exclude properties from this assembly
					var thisAssembly = GetType().Assembly;

					// as GetInterfaces doesn't return the type itself (makes sense), the following 2 lines makes a list of all
					var interfacesToCheck = new List<Type>(typeof(TInterceptor).GetInterfaces())
					{
						typeof(TInterceptor)
					};
					// Now, create an IEnumerable for all the property info of all the properties in the interfaces that the
					// "user" code introduced in the type. (e.g skip all types & properties from this assembly)
					var allPropertyInfos = from interfaceType in interfacesToCheck
						where interfaceType.Assembly != thisAssembly
						from propertyInfo in interfaceType.GetProperties()
						select propertyInfo;
					_allPropertyInfos = new Dictionary<string, PropertyInfo>(AbcComparer.Instance);
					foreach (var propertyInfo in allPropertyInfos)
					{
						_allPropertyInfos.Add(propertyInfo.Name, propertyInfo);
					}
				}

				return _allPropertyInfos;
			}
		}

		/// <summary>
		///     If an exception is catched during the initialization, it can be found here
		/// </summary>
		public IDictionary<string, Exception> InitializationErrors { get; } = new Dictionary<string, Exception>(AbcComparer.Instance);

		/// <summary>
		///     Get the raw property values of the property object
		///     Can be used to modify the directly, or for load/save
		///     Assignment to this will copy all the supplied properties.
		/// </summary>
		public IDictionary<string, object> Properties
		{
			get { return _properties; }
			set
			{
				foreach (var key in value.Keys)
				{
					_properties.SafelyAddOrOverwrite(key, value[key]);
				}
			}
		}

		#region Intercepting code

		/// <summary>
		///     Pretend the get on the property object was called
		///     This will invoke the normal get, going through all the registered getters
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		public GetInfo Get(string propertyName)
		{
			var propertyInfo = AllPropertyInfos[propertyName];

			object value;
			bool hasValue = _properties.TryGetValue(propertyName, out value);
			var getInfo = new GetInfo
			{
				PropertyName = propertyName,
				PropertyType = propertyInfo.PropertyType,
				CanContinue = true,
				Value = value,
				HasValue = hasValue
			};

			foreach (var getter in _getters)
			{
				getter.GetterAction(getInfo);
				if (!getInfo.CanContinue || getInfo.Error != null)
				{
					break;
				}
			}
			return getInfo;
		}

		/// <summary>
		///     Pretend the set on the property object was called
		///     This will invoke the normal set, going through all the registered setters
		/// </summary>
		/// <param name="propertyName">Name of the property to set</param>
		/// <param name="value">Value to set</param>
		public void Set(string propertyName, object value)
		{
			var propertyInfo = AllPropertyInfos[propertyName];

			object oldValue;
			var hasOldValue = _properties.TryGetValue(propertyName, out oldValue);
			var setInfo = new SetInfo
			{
				PropertyName = propertyName,
				PropertyType = propertyInfo.PropertyType,
				CanContinue = true,
				NewValue = value,
				HasOldValue = hasOldValue,
				OldValue = oldValue
			};

			foreach (var setter in _setters)
			{
				setter.SetterAction(setInfo);
				if (!setInfo.CanContinue || setInfo.Error != null)
				{
					break;
				}
			}
			if (setInfo.HasError && setInfo.Error != null)
			{
				throw setInfo.Error;
			}
		}

		/// <summary>
		/// The method invocation
		/// </summary>
		/// <param name="methodName"></param>
		/// <param name="parameters"></param>
		/// <returns>return value</returns>
		public object Invoke(string methodName, params object[] parameters)
		{
			// First check the methods, so we can override all other access by specifying a method
			List<Action<MethodCallInfo>> actions;
			if (_methodMap.TryGetValue(methodName, out actions))
			{
				var methodCallInfo = new MethodCallInfo
				{
					MethodName = methodName,
					Arguments = parameters
				};
				foreach (var action in actions)
				{
					action(methodCallInfo);
					if (methodCallInfo.HasError)
					{
						break;
					}
				}
				if (methodCallInfo.HasError)
				{
					throw methodCallInfo.Error;
				}
				// Note: the methodCallInfo will fix an issue here, the ReturnMessage outArgs also has the return value!!
				return methodCallInfo.ReturnValue;
			}
			throw new NotImplementedException();
		}

		#endregion


		/// <summary>
		///     Add an extension to the proxy, these extensions contain logic which enhances the proxy
		/// </summary>
		/// <param name="extensionType">Type for the extension</param>
		/// <param name="intercepted">The intercepted instance</param>
		internal void AddExtension(Type extensionType, IIntercepted intercepted)
		{
			var extension = (IInterceptorExtension) Activator.CreateInstance(extensionType.MakeGenericType(typeof(TInterceptor)));
			extension.Interceptor = this;
			extension.Intercepted = intercepted;
			extension.Initialize();
			_extensions.Add(extension);
		}

		/// <summary>
		///     A default implementation of the get logic
		/// </summary>
		/// <param name="getInfo"></param>
		private void DefaultGet(GetInfo getInfo)
		{
			object value;
			if (getInfo.PropertyName == null)
			{
				getInfo.HasValue = false;
				return;
			}
			if (_properties.TryGetValue(getInfo.PropertyName, out value))
			{
				getInfo.Value = value;
				getInfo.HasValue = true;
			}
			else
			{
				// Make sure we return the right default value, when passed by-ref there needs to be a value
				var propType = PropertyTypes[getInfo.PropertyName];
				getInfo.Value = propType.CreateInstance();
				getInfo.HasValue = false;
			}
		}

		/// <summary>
		///     A default implementation of the set logic
		/// </summary>
		/// <param name="setInfo"></param>
		private void DefaultSet(SetInfo setInfo)
		{
			var propertyType = PropertyTypes[setInfo.PropertyName];

			var newValue = propertyType.ConvertOrCastValueToType(setInfo.NewValue);
			// Add the value to the dictionary
			_properties.SafelyAddOrOverwrite(setInfo.PropertyName, newValue);
		}

		/// <summary>
		///     Get the description attribute for a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>description</returns>
		public string Description<TProp>(Expression<Func<TInterceptor, TProp>> propertyExpression)
		{
			var propertyName = propertyExpression.GetMemberName();

			var proxiedType = typeof (TInterceptor);
			var propertyInfo = proxiedType.GetProperty(propertyName);
			return propertyInfo.GetDescription();
		}

		/// <summary>
		///     This is an implementation of the Equals which returns the Equals for this
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void HandleEquals(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = Equals(methodCallInfo.Arguments[0]);
		}

		/// <summary>
		///     This is an implementation of the GetHashCode which returns the GetHashCode of this
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void HandleGetHashCode(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = GetHashCode();
		}

		/// <summary>
		///     This is an implementation of the GetType which returns the interface
		/// </summary>
		/// <param name="methodCallInfo">IMethodCallMessage</param>
		private void HandleGetType(MethodCallInfo methodCallInfo)
		{
			methodCallInfo.ReturnValue = typeof(TInterceptor);
		}

		/// <summary>
		///     Initialize, make sure every property is processed by the extensions
		/// </summary>
		internal void Init()
		{
			// Init in the right order
			var extensions = (from sortedExtension in _extensions
				orderby sortedExtension.InitOrder ascending
				select sortedExtension).ToList();

			foreach (var propertyInfo in AllPropertyInfos.Values)
			{
				PropertyTypes[propertyInfo.Name] = propertyInfo.PropertyType;

				foreach (var extension in extensions)
				{
					try
					{
						extension.InitProperty(propertyInfo);
					}
					catch (Exception ex)
					{
						Log.Warn().WriteLine(ex.Message);
						InitializationErrors.SafelyAddOrOverwrite(propertyInfo.Name, ex);
					}
				}
			}

			// Call all AfterInitialization, this allows us to ignore errors
			foreach (var extension in extensions)
			{
				extension.AfterInitialization();
			}

			// Throw if an exception was left over
			if (InitializationErrors.Count > 0)
			{
				throw InitializationErrors.Values.First();
			}
		}
	}
}