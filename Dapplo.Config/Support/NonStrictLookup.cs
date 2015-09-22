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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Dapplo.Config.Support
{
	/// <summary>
	/// This is a sorted dictionary which has non strict keys.
	/// Every key access will first convert the key by turning it to lowercase and removing everything that is not a character or digit
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NonStrictLookup<T> : IDictionary<string, T>
	{
		private IDictionary<string, T> _base = new SortedDictionary<string, T>();
		public ICollection<string> Keys
		{
			get
			{
				return _base.Keys;
            }
		}

		public ICollection<T> Values
		{
			get
			{
				return _base.Values;
			}
		}

		public int Count
		{
			get
			{
				return _base.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return _base.IsReadOnly;
			}
		}

		public bool ContainsKey(string key)
		{
			var cleanedKey = key.Cleanup();
			return _base.ContainsKey(cleanedKey);
		}

		public void Add(string key, T value)
		{
			var cleanedKey = key.Cleanup();
			_base.Add(cleanedKey, value);
		}

		public T this[string key]
		{
			get
			{
				return _base[key.Cleanup()];
			}
			set
			{
				_base[key.Cleanup()] = value;
			}
		}

		public bool TryGetValue(string key, out T value)
		{
			return _base.TryGetValue(key.Cleanup(), out value);
		}

		public bool Remove(string key)
		{
			return _base.Remove(key.Cleanup());
        }

		public void Add(KeyValuePair<string, T> item)
		{
			_base.Add(new KeyValuePair<string, T>(item.Key.Cleanup(), item.Value));
		}

		public void Clear()
		{
			_base.Clear();
        }

		public bool Contains(KeyValuePair<string, T> item)
		{
			return _base.Contains(new KeyValuePair<string, T>(item.Key.Cleanup(), item.Value));
		}

		public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
		{
			var modifiedItems = (from item in array
								 select new KeyValuePair<string, T>(item.Key.Cleanup(), item.Value)).ToArray();

            _base.CopyTo(modifiedItems, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, T> item)
		{
			return _base.Remove(new KeyValuePair<string, T>(item.Key.Cleanup(), item.Value));
		}

		public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _base.GetEnumerator();
		}
	}
}
