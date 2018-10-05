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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dapplo.Config.Interfaces;

#endregion

namespace Dapplo.Ini
{
	/// <summary>
	///     By making your property proxy interface extend this, you will be able to write the property to an ini file
	/// </summary>
	public interface IIniSection : IConfiguration
	{
		/// <summary>
		///     This is called after the loading of the IniSection is finished and can be used to modify certain values before they are being used.
		/// </summary>
		void AfterLoad();

		/// <summary>
		///     This is called after the saving of the IniSection is finished and can be used to modify certain values
		/// </summary>
		void AfterSave();

		/// <summary>
		///     This is called before the saving of the IniSection is started and can be used to modify certain values
		/// </summary>
		void BeforeSave();

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
	}
}