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

using System.Collections.Generic;

#endregion

namespace Dapplo.Config.Support
{
	public static class DictionaryExtensions
	{
		/// <summary>
		///     Safely add or overwrite a value in the dictionary, supply the key & value
		/// </summary>
		/// <param name="dictionary"></param>
		/// <param name="key">string key</param>
		/// <param name="newValue">object</param>
		/// <returns>dictionary for fluent API calls</returns>
		public static IDictionary<T1, T2> SafelyAddOrOverwrite<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 newValue)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = newValue;
			}
			else
			{
				dictionary.Add(key, newValue);
			}
			return dictionary;
		}
	}
}