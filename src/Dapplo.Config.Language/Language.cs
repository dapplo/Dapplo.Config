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

using Dapplo.Config.Language.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapplo.Config.Intercepting;

namespace Dapplo.Config.Language
{
	/// <summary>
	///     Base Language functionality
	/// </summary>
	public class Language<TInterface> : DictionaryConfigurationBase<TInterface, string>, ILanguage, ILanguageInternal
    {
		private readonly LanguageAttribute _languageAttribute = typeof(TInterface).GetCustomAttribute<LanguageAttribute>();
        private readonly IDictionary<string, string> _translationsWithoutProperty = new Dictionary<string, string>(AbcComparer.Instance);

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

        }

        /// <summary>
        /// Set via the DictionaryConfiguration when supported by a property
        /// or use the _translationsWithoutPropery if not.
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

            // Store values which do not have a property
            _translationsWithoutProperty[propertyName] = translation;
        }

        /// <summary>
        /// Override for the getter, which takes care of properties without backing property
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>object</returns>
        public override object Getter(string propertyName)
        {
	        // Check if the property has a matching PropertyInfo, if so use the GetValue to let the chain of getters work
	        if (TryGetTranslation(propertyName, out var translation))
	        {
		        return translation;
	        }
            throw new NotSupportedException($"No property with the name {propertyName} found");
        }

        /// <summary>
        ///     Get the value for a property.
        /// Note: This needs to be virtual otherwise the interface isn't implemented
        /// </summary>
        /// <param name="propertyName">string with propertyName for the property to get</param>
        /// <returns>object or null if not available</returns>
        public override string this[string propertyName]
        {
            get => (string)Getter(propertyName);
            set => Setter(propertyName, value);
        }

        /// <summary>
        ///     This event is triggered after the language has been changed
        /// </summary>
        public event EventHandler<EventArgs> LanguageChanged;

	    /// <inheritdoc />
	    public bool TryGetTranslation(string languageKey, out string translation)
	    {
		    if (TryGetPropertyInfoFor(languageKey, out var propertyInfo))
		    {
			    translation = GetValue(propertyInfo).Value;
			    return true;
		    }

		    // This property is not backed by a Property, use the local dictionary
		    if (_translationsWithoutProperty.TryGetValue(languageKey, out translation))
		    {
			    return true;
            }

		    return false;
	    }

	    /// <inheritdoc />
		public IEnumerable<string> Keys()
		{
			return PropertyNames().Concat(_translationsWithoutProperty.Keys);
		}

	    /// <inheritdoc />
	    public IEnumerable<string> PropertyFreeKeys()
	    {
		    return _translationsWithoutProperty.Keys;
	    }

        /// <inheritdoc />
        public string PrefixName()
		{
			return _languageAttribute?.Prefix ?? typeof(TInterface).Name;
		}

		/// <summary>
		///     Generate a LanguageChanged event, this is from ILanguageInternal
		///     Added for Dapplo.Config/issues/10
		/// </summary>
		public void OnLanguageChanged()
		{
			LanguageChanged?.Invoke(this, new EventArgs());
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