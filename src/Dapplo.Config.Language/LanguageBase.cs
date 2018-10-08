//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
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

using Dapplo.Config.Language.Implementation;
using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Dapplo.Config.Language
{
	/// <summary>
	///     Base Language functionality
	/// </summary>
	public class LanguageBase<T> : DictionaryConfigurationBase<T, string>, ILanguage, ILanguageInternal
    {
		private readonly LanguageAttribute _languageAttribute = typeof(T).GetCustomAttribute<LanguageAttribute>();

		/// <summary>
		///     This event is triggered after the language has been changed
		/// </summary>
		public event EventHandler<EventArgs> LanguageChanged;

	    /// <inheritdoc />
		public IEnumerable<string> Keys()
		{
			return PropertyNames();
		}

		/// <inheritdoc />
		public string PrefixName()
		{
			return _languageAttribute?.Prefix ?? typeof(T).Name;
		}

		/// <summary>
		///     Generate a LanguageChanged event, this is from ILanguageInternal
		///     Added for Dapplo.Config/issues/10
		/// </summary>
		public void OnLanguageChanged()
		{
			LanguageChanged?.Invoke(this, new EventArgs());
		}

	    protected override void PropertyInitializer(PropertyInfo propertyInfo)
	    {
		    base.PropertyInitializer(propertyInfo);
		    if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
		    {
			    throw new NotSupportedException(
				    $"Property {propertyInfo.DeclaringType}.{propertyInfo.Name} has defined a set, this is not allowed for {typeof(ILanguage).Name} derrived interfaces. Fix by removing the set for the property, leave the get.");
		    }
        }
	}
}