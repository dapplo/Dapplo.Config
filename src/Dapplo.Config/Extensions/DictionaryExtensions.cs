//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Dapplo.Config.Extensions
{
	/// <summary>
	///     A few Dictionary helper extensions, e.g. used in FormatWith
	/// </summary>
	public static class DictionaryExtensions
	{
		/// <summary>
		///     Only add when the key isn't in the dictionary yet
		/// </summary>
		/// <typeparam name="TKey">type for the Key</typeparam>
		/// <typeparam name="TValue">type for the value</typeparam>
		/// <param name="dictionary">IDictionary</param>
		/// <param name="key">new key of type TKey</param>
		/// <param name="value">value of type TValue</param>
		/// <returns>IDictionary so fluent calls are possible</returns>
		public static IDictionary<TKey, TValue> AddWhenNew<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			if (!dictionary.ContainsKey(key))
			{
				dictionary.Add(key, value);
			}
			return dictionary;
		}

		/// <summary>
		///     Map a dictionary to properties
		/// </summary>
		/// <param name="properties">IDictionary with properties to add to</param>
		/// <param name="dictionary">dictionary to process, or null due to "as" cast</param>
		/// <returns>false if dictionary was null</returns>
		public static bool DictionaryToGenericDictionary<TKey, TValue>(this IDictionary<TKey, TValue> properties, IDictionary dictionary)
		{
			if (dictionary == null)
			{
				return false;
			}

			var dictionaryType = dictionary.GetType().GetTypeInfo();
			if (!dictionaryType.IsGenericType || dictionaryType.GenericTypeArguments[0] != typeof(TKey))
			{
				return true;
			}
			foreach (DictionaryEntry item in dictionary)
			{
				var key = (TKey) item.Key;
				var value = (TValue) item.Value;
				properties.AddWhenNew(key, value);
			}
			// Also return true if the dictionary didn't have keys of type string, as we don't know what to do with it.
			return true;
		}
	}
}