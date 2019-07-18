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

using System;
using System.Reflection;
using Dapplo.Config.Intercepting;
using Dapplo.Config.Language.ConfigExtensions;

namespace Dapplo.Config.Language
{
	/// <summary>
	///     Base Language functionality
	/// </summary>
	public class Language<TInterface> : DictionaryConfiguration<TInterface, string>
	{
		private readonly LanguageExtension<string> _languageExtension;

        /// <summary>
        /// Factory for IniSectionBase implementations
        /// </summary>
        /// <returns>TInterface</returns>
        public static TInterface Create()
        {
	        return ConfigProxy.Create<TInterface>(new Language<TInterface>());
        }

        /// <summary>
        /// Prevent new-ing
        /// </summary>
        protected Language()
        {
	        _languageExtension = new LanguageExtension<string>(this);
        }

        /// <summary>
        /// Set via the DictionaryConfiguration when supported by a property
        /// or use the _translationsWithoutProperty if not.
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="newValue">object</param>
        public override void Setter(string propertyName, object newValue)
        {
            var translation = (string)newValue;

			// Check if the property is backed by a Property by checking if it has a matching PropertyInfo, if so use the SetValue to let the chain of setters work
            if (TryGetPropertyInfoFor(propertyName, out var propertyInfo))
            {
                SetValue(propertyInfo, translation);
                return;
            }
			_languageExtension.SetPropertyFreeTranslation(propertyName, translation);
        }

        /// <summary>
        /// Override for the getter, which takes care of properties without backing property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>object</returns>
        public override object Getter(string propertyName)
        {
	        // Check if the property has a matching PropertyInfo, if so use the GetValue to let the chain of getters work
	        if (_languageExtension.TryGetTranslation(propertyName, out var translation))
	        {
		        return translation;
	        }
            throw new NotSupportedException($"No property with the name {propertyName} found");
        }

	    /// <inheritdoc />
	    protected override void PropertyInitializer(PropertyInfo propertyInfo)
	    {
		    base.PropertyInitializer(propertyInfo);
		    if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
		    {
			    throw new NotSupportedException(
				    $"Property {propertyInfo.DeclaringType}.{propertyInfo.Name} has defined a set, this is not allowed for {typeof(ILanguage).Name} derived interfaces. Fix by removing the set for the property, leave the get.");
		    }
        }
	}
}