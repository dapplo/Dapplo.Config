// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Dapplo.Config.Interfaces;


namespace Dapplo.Config.Ini
{
	/// <summary>
	///     By making your property proxy interface extend this, you will be able to write the property to an ini file
	/// </summary>
	public interface IIniSection : IConfiguration<object>
	{
        /// <summary>
        ///     Get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>IniValue</returns>
        IniValue GetIniValue(string propertyName);

		/// <summary>
		///     Retrieve all the ini values
		/// </summary>
		/// <returns>readonly dictionary</returns>
		IReadOnlyDictionary<string, IniValue> GetIniValues();

		/// <summary>
		///     Get the Description of the Ini-Section
		/// </summary>
		string GetSectionDescription();

		/// <summary>
		///     Name of the Ini-Section, should be set on your property interface with
		/// </summary>
		string GetSectionName();

		/// <summary>
		///     Try to get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues
		///     result
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="value">out IniValue</param>
		/// <returns>bool with true if found</returns>
		bool TryGetIniValue(string propertyName, out IniValue value);

        /// <summary>
        /// This is the setter of the configuration base, made public
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <param name="value">object</param>
        void Setter(string propertyName, object value);

        /// <summary>
        /// This is the getter of the configuration base, made public
        /// </summary>
        /// <param name="propertyName">string</param>
        /// <returns>object</returns>
        object Getter(string propertyName);

        /// <summary>
        ///     This is called after the loading of the IniSection is finished and can be used to modify certain values before they are being used.
        /// </summary>
        void RegisterAfterLoad(Action<IIniSection> onAfterLoad);

        /// <summary>
        ///     This is called after the saving of the IniSection is finished and can be used to modify certain values
        /// </summary>
        void RegisterAfterSave(Action<IIniSection> onAfterSave);

        /// <summary>
        ///     This is called before the saving of the IniSection is started and can be used to modify certain values
        /// </summary>
        void RegisterBeforeSave(Action<IIniSection> onBeforeSave);
	}
}