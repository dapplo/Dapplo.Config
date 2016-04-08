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
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Collections.Generic;
using System.Reflection;
using Dapplo.InterfaceImpl;
using Dapplo.InterfaceImpl.Implementation;

#endregion

namespace Dapplo.Config.Language.Implementation
{
	/// <summary>
	///     Base Language functionality
	/// </summary>
	public class Language<T> : ExtensibleInterceptorImpl<T>, ILanguage
	{
		private readonly LanguageAttribute _languageAttribute = typeof (T).GetCustomAttribute<LanguageAttribute>();

		/// <summary>
		///     Get a translation for a property
		/// </summary>
		/// <param name="languageKey"></param>
		/// <returns>string or null if not available</returns>
		public string this[string languageKey]
		{
			get
			{
				object translation;
				Properties.TryGetValue(languageKey, out translation);
				return (string) translation;
			}
		}

		/// <summary>
		///     All available keys for the language object
		/// </summary>
		/// <returns>collection</returns>
		public ICollection<string> Keys()
		{
			return Properties.Keys;
		}

		/// <summary>
		///     The prefix
		/// </summary>
		/// <returns>string</returns>
		public string PrefixName()
		{
			return _languageAttribute?.Prefix ?? typeof (T).Name;
		}

		/// <summary>
		///     Logic to check every property for read only
		/// </summary>
		/// <param name="propertyInfo"></param>
		/// <param name="extensions"></param>
		protected override void InitProperty(PropertyInfo propertyInfo, IList<IInterceptorExtension> extensions)
		{
			base.InitProperty(propertyInfo, extensions);
			if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
			{
				throw new NotSupportedException(
					$"Property {propertyInfo.DeclaringType}.{propertyInfo.Name} has defined a set, this is not allowed for {typeof (ILanguage).Name} derrived interfaces. Fix by removing the set for the property, leave the get.");
			}
		}
	}
}