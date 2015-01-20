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
using System.Linq;

namespace Dapplo.Config {
	public class ProxyBuilder {
		private static readonly List<Type> ExtensionTypes = new List<Type>();

		static ProxyBuilder() {
			IEnumerable<Type> types = from someAssembly in AppDomain.CurrentDomain.GetAssemblies() from someType in someAssembly.GetTypes() where someType.GetCustomAttributes(typeof (ExtensionAttribute), true).Length > 0 select someType;
			ExtensionTypes.AddRange(types);
		}

		private ProxyBuilder() {
		}

		/// <summary>
		///     This method creates a proxy for the given type.
		///     If the type implements certain interfaces, that are known, the matching proxy extensions are automatically added.
		/// </summary>
		/// <typeparam name="T">Should be an interface</typeparam>
		/// <returns>proxy</returns>
		public static IPropertyProxy<T> CreateProxy<T>() {
			IPropertyProxy<T> proxy = new PropertyProxy<T>();
			Type[] interfaces = typeof (T).GetInterfaces();
			foreach (Type extensionType in ExtensionTypes) {
				var extensionAttributes = (ExtensionAttribute[]) extensionType.GetCustomAttributes(typeof (ExtensionAttribute), false);
				foreach (ExtensionAttribute extensionAttribute in extensionAttributes) {
					Type implementing = extensionAttribute.Implementing;
					if (interfaces.Contains(implementing)) {
						proxy.AddExtension(extensionType);
					} else if (implementing.IsGenericType && implementing.IsGenericTypeDefinition) {
						Type genericExtensionType = implementing.MakeGenericType(typeof (T));
						if (interfaces.Contains(genericExtensionType)) {
							proxy.AddExtension(extensionType);
						}
					}
				}
			}

			return proxy;
		}
	}
}