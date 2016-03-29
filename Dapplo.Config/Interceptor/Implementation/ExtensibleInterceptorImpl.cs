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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dapplo.Config.Support;
using Dapplo.LogFacade;
using System.Collections.ObjectModel;

#endregion

namespace Dapplo.Config.Interceptor.Implementation
{
	/// <summary>
	///     Implementation of the IInterceptor
	/// </summary>
	public class ExtensibleInterceptorImpl<T> : IExtensibleInterceptor
	{
		// ReSharper disable once StaticMemberInGenericType
		private static readonly LogSource Log = new LogSource();
		private readonly IList<IInterceptorExtension> _extensions = new List<IInterceptorExtension>();
		private readonly IList<Getter> _getters = new List<Getter>();
		private readonly IDictionary<string, List<Action<MethodCallInfo>>> _methodMap = new Dictionary<string, List<Action<MethodCallInfo>>>();
		private readonly IDictionary<string, object> _properties = new Dictionary<string, object>(AbcComparer.Instance);
		private IReadOnlyDictionary<string, Type> _propertyTypes;
		private readonly IList<Setter> _setters = new List<Setter>();

		/// <summary>
		///     Constructor
		/// </summary>
		public ExtensibleInterceptorImpl()
		{
			// Make sure the default set logic is registered
			RegisterSetter((int) CallOrder.Middle, DefaultSet);
			// Make sure the default get logic is registered
			RegisterGetter((int) CallOrder.Middle, DefaultGet);
		}

		#region IInterceptor

		/// <summary>
		///     If an exception is catched during the initialization, it can be found here
		/// </summary>
		public IDictionary<string, Exception> InitializationErrors { get; } = new Dictionary<string, Exception>(AbcComparer.Instance);

		/// <summary>
		///     Get type of a property
		/// </summary>
		public IReadOnlyDictionary<string, Type> PropertyTypes => _propertyTypes;

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

		public Type InterceptedType => typeof(T);


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
			((List<Setter>)_setters).Sort();
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
			((List<Getter>)_getters).Sort();
		}
		#endregion

		#region Intercepting code

		/// <summary>
		///     Pretend the get on the property object was called
		///     This will invoke the normal get, going through all the registered getters
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		public GetInfo Get(string propertyName)
		{
			var propertyType = PropertyTypes[propertyName];

			object value;
			bool hasValue = _properties.TryGetValue(propertyName, out value);
			var getInfo = new GetInfo
			{
				PropertyName = propertyName,
				PropertyType = propertyType,
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
			var propertyInfo = PropertyTypes[propertyName];

			object oldValue;
			var hasOldValue = _properties.TryGetValue(propertyName, out oldValue);
			var setInfo = new SetInfo
			{
				PropertyName = propertyName,
				PropertyType = propertyInfo,
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
				// TODO: make out parameters possible
				return methodCallInfo.ReturnValue;
			}
			throw new NotImplementedException();
		}

		#endregion


		/// <summary>
		///     Add an extension to the proxy, these extensions contain logic which enhances the proxy
		/// </summary>
		/// <param name="extensionType">Type for the extension</param>
		public void AddExtension(Type extensionType)
		{
			if (extensionType.IsGenericType)
			{
				extensionType = extensionType.MakeGenericType(typeof(T));
			}

			var extension = (IInterceptorExtension) Activator.CreateInstance(extensionType);
			extension.Interceptor = this;
			extension.Initialize();
			_extensions.Add(extension);
		}

		#region Default Get/Set
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
			var propertyType = _propertyTypes[setInfo.PropertyName];

			var newValue = propertyType.ConvertOrCastValueToType(setInfo.NewValue);
			// Add the value to the dictionary
			_properties.SafelyAddOrOverwrite(setInfo.PropertyName, newValue);
		}
		#endregion

		/// <summary>
		///     Get the description attribute for a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>description</returns>
		public string Description<TProp>(Expression<Func<T, TProp>> propertyExpression)
		{
			var propertyName = propertyExpression.GetMemberName();
			return Description(propertyName);
		}

		/// <summary>
		///     Get the description attribute for a property
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>description</returns>
		public string Description(string propertyName)
		{
			var proxiedType = typeof (T);
			var propertyInfo = proxiedType.GetProperty(propertyName);
			return propertyInfo.GetDescription();
		}

		/// <summary>
		///     Initialize, make sure every property is processed by the extensions
		/// </summary>
		public virtual void Init()
		{
			// Init in the right order
			var extensions = (from sortedExtension in _extensions
				orderby sortedExtension.InitOrder ascending
				select sortedExtension).ToList();

			// Exclude properties from this assembly
			var thisAssembly = GetType().Assembly;

			// as GetInterfaces doesn't return the type itself (makes sense), the following 2 lines makes a list of all
			var interfacesToCheck = new List<Type>(typeof(T).GetInterfaces())
					{
						typeof(T)
					};

			var propertyTypes = new Dictionary<string, Type>(AbcComparer.Instance);
			_propertyTypes = new ReadOnlyDictionary<string, Type>(propertyTypes);

			// Now, create an IEnumerable for all the property info of all the properties in the interfaces that the
			// "user" code introduced in the type. (e.g skip all types & properties from this assembly)
			var allPropertyInfos = (from interfaceType in interfacesToCheck
									where interfaceType.Assembly != thisAssembly
									from propertyInfo in interfaceType.GetProperties()
									select propertyInfo).GroupBy(p => p.Name).Select(group => group.First());

			foreach (var propertyInfo in allPropertyInfos)
			{
				propertyTypes.Add(propertyInfo.Name, propertyInfo.PropertyType);
				InitProperty(propertyInfo, extensions);
			}

			AfterInitialization(extensions);

			// Throw if an exception was left over
			if (InitializationErrors.Count > 0)
			{
				throw InitializationErrors.Values.First();
			}
		}

		protected virtual void InitProperty(PropertyInfo propertyInfo, IList<IInterceptorExtension> extensions)
		{
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

		protected virtual void AfterInitialization(IList<IInterceptorExtension> extensions)
		{
			// Call all AfterInitialization, this allows us to ignore errors
			foreach (var extension in extensions)
			{
				extension.AfterInitialization();
			}
		}
	}
}