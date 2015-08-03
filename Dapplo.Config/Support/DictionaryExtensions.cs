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

using System.Collections.Generic;

namespace Dapplo.Config.Support {
	public static class DictionaryExtensions {
		/// <summary>
		/// Safely retrieve a value from the dictionary, by using a key
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key"></param>
		/// <returns>object</returns>
		public static object SafeGet(this IDictionary<string, object> dictionary, string key) {
			object value;
			if (dictionary.TryGetValue(key, out value)) {
				return value;
			}
			return null;
		}

		/// <summary>
		/// Safely add or overwrite a value in the dictionary, supply the key & value
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">string key</param>
		/// <param name="newValue">object</param>
		/// <returns>dictionary for fluent API calls</returns>
		public static IDictionary<T1, T2> SafelyAddOrOverwrite<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 newValue) {
			if (dictionary.ContainsKey(key)) {
				dictionary[key] = newValue;
			} else {
				dictionary.Add(key, newValue);
			}
			return dictionary;
		}
	}
}
