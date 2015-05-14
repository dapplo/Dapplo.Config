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
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Dapplo.Config.WindowsRegistry {
	public class RegistryConfig {
		private readonly SemaphoreSlim _sync = new SemaphoreSlim(1);
		private readonly IDictionary<Type, IRegistry> _registryConfigs = new SortedDictionary<Type, IRegistry>();

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <typeparam name="T">Your property interface, which extends IIniSection</typeparam>
		/// <returns>instance of type T</returns>
		public async Task<T> RegisterAndGetAsync<T>(CancellationToken token = default(CancellationToken)) where T : IRegistry {
			return (T)await RegisterAndGetAsync(typeof(T), token);
		}

		/// <summary>
		/// Register the supplied types
		/// </summary>
		/// <param name="types">Types to register, these must extend IIniSection</param>
		/// <returns>List with instances for the supplied types</returns>
		public async Task<IList<IRegistry>> RegisterAndGetAsync(IEnumerable<Type> types, CancellationToken token = default(CancellationToken)) {
			IList<IRegistry> registryConfigs = new List<IRegistry>();
			foreach (var type in types) {
				registryConfigs.Add(await RegisterAndGetAsync(type, token));
			}
			return registryConfigs;
		}

		/// <summary>
		/// Register a Property Interface to this ini config, this method will return the property object 
		/// </summary>
		/// <param name="type">Type to register, this must extend IIniSection</param>
		/// <returns>instance of type</returns>
		public async Task<IRegistry> RegisterAndGetAsync(Type type, CancellationToken token = default(CancellationToken)) {
			if (!typeof(IRegistry).IsAssignableFrom(type)) {
				throw new ArgumentException("type is not a IRegistry");
			}
			var _propertyProxy = ProxyBuilder.GetOrCreateProxy(type);
			var registryConfig = (IRegistry)_propertyProxy.PropertyObject;

			using (await Sync.Wait(_sync)) {
				if (!_registryConfigs.ContainsKey(type)) {
					// TODO: Read values
					_registryConfigs.Add(type, registryConfig);
				}
			}

			return registryConfig;
		}
	}
}
