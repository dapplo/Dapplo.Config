/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Extension;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// By making your property proxy interface extend this, you will be able to write the property to an ini file
	/// </summary>
	public interface IIniSection : IDefaultValue, IWriteProtectProperties
	{
		/// <summary>
		/// Retrieve all the ini values
		/// </summary>
		/// <returns></returns>
		IEnumerable<IniValue> GetIniValues();

		/// <summary>
		/// Name of the Ini-Section, should be set on your property interface with
		/// </summary>
		string GetSectionName();

		/// <summary>
		/// Get the Description of the Ini-Section
		/// </summary>
		string GetSectionDescription();

		/// <summary>
		/// Get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>IniValue</returns>
		IniValue GetIniValue(string propertyName);

		/// <summary>
		/// Try to get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="propertyName">out IniValue</param>
		/// <returns>bool with true if found</returns>
		bool TryGetIniValue(string propertyName, out IniValue iniValue);
	}

	/// <summary>
	/// Generic version of IIniSection
	/// </summary>
	public interface IIniSection<T> : IIniSection, IDefaultValue<T>, IWriteProtectProperties<T>
	{
		/// <summary>
		/// Get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
		/// </summary>
		/// <typeparam name="TProp">Your interface</typeparam>
		/// <param name="propertyExpression">expression for the property name</param>
		/// <returns>IniValue</returns>
		IniValue GetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		/// Try to get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
		/// </summary>
		/// <typeparam name="TProp">Your interface</typeparam>
		/// <param name="propertyExpression">expression for the property name</param>
		/// <param name="iniValue">out IniValue</param>
		/// <returns>bool with true if found</returns>
		bool TryGetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression, out IniValue iniValue);
	}
}