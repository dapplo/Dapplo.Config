// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.Language
{
	/// <summary>
	///     The base interface for all language objects.
	///     My advice is that you extend your inteface with this, and the INotifyPropertyChanged,
	///     so language changes are directly reflected in the UI.
	///     This extends IDefaultValue, as this it is very common to start with default translations.
	///     These defaults, usually en-US, can be set with the DefaultValueAttribute
	/// </summary>
	public interface ILanguage : IConfiguration<string>
	{
		/// <summary>
		///     Get the translation for a key
		/// </summary>
		/// <param name="languageKey">Key for the translation</param>
		/// <returns>string or null for the translation</returns>
		string this[string languageKey] { get; }

		/// <summary>
        /// Tries to get a translation
        /// </summary>
        /// <param name="languageKey">key to translate</param>
        /// <param name="translation">value for the translation</param>
        /// <returns>bool true if the key was available, false if not</returns>
        bool TryGetTranslation(string languageKey, out string translation);

        /// <summary>
        ///     Get all the language keys, this includes also the ones that don't have an access property.
        ///     This is a method, could have been a property, but this differentiates it from the properties in the extending
        ///     interface.
        /// </summary>
        /// <returns>IEnumerable of string</returns>
        IEnumerable<string> Keys();

		/// <summary>
		///     Get all the language keys which don't have an access property.
		/// </summary>
		/// <returns>IEnumerable of string</returns>
        IEnumerable<string> PropertyFreeKeys();

        /// <summary>
        ///     This event is triggered after the language has been changed
        ///     Added for Dapplo.Config/issues/10
        /// </summary>
        event EventHandler<EventArgs> LanguageChanged;

		/// <summary>
		///     Get the prefix / module name of this ILanguage
		/// </summary>
		/// <returns>string</returns>
		string PrefixName();
	}
}