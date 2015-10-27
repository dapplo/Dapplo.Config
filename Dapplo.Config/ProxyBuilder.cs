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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dapplo.Config
{
	/// <summary>
	/// This is the proxy builder
	/// The ProxyBuilder is used to create instances of interfaces with implementations depending on
	/// the extended interfaces. Especially useful for configurations.
	/// </summary>
	public static class ProxyBuilder
	{
		private static readonly List<Type> ExtensionTypes = new List<Type>();
		private static readonly IDictionary<Type, IPropertyProxy> Cache = new ConcurrentDictionary<Type, IPropertyProxy>();

		static ProxyBuilder()
		{
			IEnumerable<Type> types =
				from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where !assembly.FullName.StartsWith("System") && !assembly.FullName.StartsWith("mscorlib") && !assembly.FullName.StartsWith("Microsoft")
					from someType in assembly.GetTypes()
					where someType.GetCustomAttributes(typeof (ExtensionAttribute), true).Length > 0
					select someType;
			ExtensionTypes.AddRange(types);
		}

		/// <summary>
		/// Delete a proxy
		/// This is interal, mainly for tests, normally it should not be needed.
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		public static void DeleteProxy<T>()
		{
			DeleteProxy(typeof(T));
		}

		/// <summary>
		/// Delete a proxy
		/// This is interal, mainly for tests, normally it should not be needed.
		/// </summary>
		/// <param name="proxyType">Should be an interface</typeparam>
		public static void DeleteProxy(Type proxyType)
		{
			lock (Cache)
			{
				Cache.Remove(proxyType);
			}
		}

		/// <summary>
		/// Get a proxy, this throws an exception if it doesn't exist
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		/// <returns>proxy</returns>
		public static IPropertyProxy<T> GetProxy<T>()
        {
            return (IPropertyProxy<T>)GetProxy(typeof(T));
        }

        /// <summary>
        /// Get a proxy, this throws an exception if it doesn't exist
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

        /// <summary>
        /// This can be used for Caching the Proxy generation, if there is only one proxy instance needed you want to call this!
        /// </summary>
        /// <typeparam name="T">Should be an interface</typeparam>
        /// <returns>proxy</returns>
        public static IPropertyProxy<T> GetOrCreateProxy<T>()
		{
			return (IPropertyProxy<T>) GetOrCreateProxy(typeof (T));
		}

        /// <summary>
        /// This can be used for Caching the Proxy generation, if there is only one proxy instance needed you want to call this!
        /// </summary>
        /// <param name="type">Type to create </param>
        /// <returns>proxy</returns>
        public static IPropertyProxy GetOrCreateProxy(Type type)
        {
            IPropertyProxy proxy;
            lock (Cache)
            {
                if (!Cache.TryGetValue(type, out proxy))
                {
                    proxy = CreateProxy(type);
                    Cache.Add(type, proxy);
                }
            }
            return proxy;
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
		/// <param name="type">Type to create </param>
		/// <returns>proxy</returns>
		public static IPropertyProxy CreateProxy(Type type)
		{
			var genericType = typeof (PropertyProxy<>).MakeGenericType(type);
			var proxy = (IPropertyProxy) Activator.CreateInstance(genericType, null);
			Type[] interfaces = type.GetInterfaces();
			var addExtensionMethodInfo = genericType.GetMethod("AddExtension", BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (Type extensionType in ExtensionTypes)
			{
				var extensionAttributes = (ExtensionAttribute[]) extensionType.GetCustomAttributes(typeof (ExtensionAttribute), false);
				foreach (ExtensionAttribute extensionAttribute in extensionAttributes)
				{
					Type implementing = extensionAttribute.Implementing;
					if (interfaces.Contains(implementing))
					{
						addExtensionMethodInfo.Invoke(proxy, new object[]
						{
							extensionType
						});
					}
					else if (implementing.IsGenericType && implementing.IsGenericTypeDefinition)
					{
						Type genericExtensionType = implementing.MakeGenericType(type);
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
			try {
				genericType.GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(proxy, null);
			} catch (TargetInvocationException ex) {
				throw ex.InnerException;
			}
			return proxy;
		}
	}
}