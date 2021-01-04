// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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