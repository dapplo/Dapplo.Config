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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapplo.LogFacade;

#endregion

namespace Dapplo.Config
{
	/// <summary>
	///     This is the proxy builder
	///     The ProxyBuilder is used to create instances of interfaces with implementations depending on
	///     the extended interfaces. Especially useful for configurations.
	/// </summary>
	public static class ProxyBuilder
	{
		private static readonly LogSource Log = new LogSource();
		private static readonly List<Type> ExtensionTypes = new List<Type>();
		private static readonly IDictionary<Type, IPropertyProxy> Cache = new ConcurrentDictionary<Type, IPropertyProxy>();

		static ProxyBuilder()
		{
			var types =
				from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where
					!assembly.FullName.StartsWith("System") && !assembly.FullName.StartsWith("mscorlib") && !assembly.FullName.StartsWith("Microsoft") &&
					!assembly.FullName.StartsWith("xunit")
				from someType in assembly.GetTypes()
				where someType.GetCustomAttributes(typeof (ExtensionAttribute), true).Length > 0
				select someType;
			ExtensionTypes.AddRange(types);
		}

		/// <summary>
		///     This method creates a proxy for the given type.
		///     If the type implements certain interfaces, that are known, the matching proxy extensions are automatically added.
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		/// <returns>proxy</returns>
		public static IPropertyProxy<T> CreateProxy<T>()
		{
			return (IPropertyProxy<T>) CreateProxy(typeof (T));
		}

		/// <summary>
		///     This method creates a proxy for the given type.
		///     If the type implements certain interfaces, that are known, the matching proxy extensions are automatically added.
		/// </summary>
		/// <param name="proxyType">Type to create </param>
		/// <returns>proxy</returns>
		public static IPropertyProxy CreateProxy(Type proxyType)
		{
			Log.Debug().WriteLine("Creating proxy {0}", proxyType.FullName);
			var genericType = typeof (PropertyProxy<>).MakeGenericType(proxyType);
			var proxy = (IPropertyProxy) Activator.CreateInstance(genericType, null);
			var interfaces = proxyType.GetInterfaces();
			var addExtensionMethodInfo = genericType.GetMethod("AddExtension", BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (var extensionType in ExtensionTypes)
			{
				var extensionAttributes = (ExtensionAttribute[]) extensionType.GetCustomAttributes(typeof (ExtensionAttribute), false);
				foreach (var extensionAttribute in extensionAttributes)
				{
					var implementing = extensionAttribute.Implementing;
					if (interfaces.Contains(implementing))
					{
						addExtensionMethodInfo.Invoke(proxy, new object[]
						{
							extensionType
						});
					}
					else if (implementing.IsGenericType && implementing.IsGenericTypeDefinition)
					{
						var genericExtensionType = implementing.MakeGenericType(proxyType);
						if (interfaces.Contains(genericExtensionType))
						{
							addExtensionMethodInfo.Invoke(proxy, new object[]
							{
								extensionType
							});
						}
					}
				}
			}
			// Call the init, this will also process any extensions
			try
			{
				genericType.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(proxy, null);
			}
			catch (TargetInvocationException ex)
			{
				// Ignore creating the default type, this might happen if there is no default constructor.
				Log.Warn().WriteLine(ex.Message);
				throw ex.InnerException;
			}
			return proxy;
		}

		/// <summary>
		///     Delete a proxy
		///     This is interal, mainly for tests, normally it should not be needed.
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		public static void DeleteProxy<T>()
		{
			DeleteProxy(typeof (T));
		}

		/// <summary>
		///     Delete a proxy
		///     This is interal, mainly for tests, normally it should not be needed.
		/// </summary>
		/// <param name="proxyType">Should be an interface</param>
		public static void DeleteProxy(Type proxyType)
		{
			lock (Cache)
			{
				Log.Debug().WriteLine("Removing proxyType {0} from cache.", proxyType.FullName);
				Cache.Remove(proxyType);
			}
		}

		/// <summary>
		///     This can be used for Caching the Proxy generation, if there is only one proxy instance needed you want to call
		///     this!
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		/// <returns>proxy</returns>
		public static IPropertyProxy<T> GetOrCreateProxy<T>()
		{
			return (IPropertyProxy<T>) GetOrCreateProxy(typeof (T));
		}

		/// <summary>
		///     This can be used for Caching the Proxy generation, if there is only one proxy instance needed you want to call
		///     this!
		/// </summary>
		/// <param name="proxyType">Type to create </param>
		/// <returns>proxy</returns>
		public static IPropertyProxy GetOrCreateProxy(Type proxyType)
		{
			IPropertyProxy proxy;
			lock (Cache)
			{
				if (!Cache.TryGetValue(proxyType, out proxy))
				{
					proxy = CreateProxy(proxyType);
					Log.Debug().WriteLine("Adding proxy {0} to cache.", proxyType.FullName);
					Cache.Add(proxyType, proxy);
				}
			}
			return proxy;
		}

		/// <summary>
		///     Get a proxy, this throws an exception if it doesn't exist
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		/// <returns>proxy</returns>
		public static IPropertyProxy<T> GetProxy<T>()
		{
			return (IPropertyProxy<T>) GetProxy(typeof (T));
		}

		/// <summary>
		///     Get a proxy, this throws an exception if it doesn't exist
		/// </summary>
		/// <param name="type">Type to get</param>
		/// <returns>proxy</returns>
		public static IPropertyProxy GetProxy(Type type)
		{
			IPropertyProxy proxy;
			lock (Cache)
			{
				if (!Cache.TryGetValue(type, out proxy))
				{
					throw new KeyNotFoundException(type.FullName);
				}
			}
			return proxy;
		}
	}
}