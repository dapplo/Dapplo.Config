//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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

#endregion

namespace Dapplo.Ini
{
	/// <summary>
	/// </summary>
	internal interface IIniSectionInternal
	{
		/// <summary>
		///     Generate the Loaded event
		/// </summary>
		void OnLoaded();

		/// <summary>
		///     Generate the Reset event
		/// </summary>
		void OnReset();

		/// <summary>
		///     Generate the Saved event
		/// </summary>
		void OnSaved();

		/// <summary>
		///     Generate the Saving event
		/// </summary>
		void OnSaving();
	}

	/// <summary>
	///     By making your property proxy interface extend this, you will be able to write the property to an ini file
	/// </summary>
	public interface IIniSection
	{
		/// <summary>
		///     Get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>IniValue</returns>
		IniValue this[string propertyName] { get; }

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
		///     The loaded event is triggered after the file was changed or reload was called
		/// </summary>
		event EventHandler<IniSectionEventArgs> Loaded;

		/// <summary>
		///     The reset event is triggered when the IIniSection was reset
		/// </summary>
		event EventHandler<IniSectionEventArgs> Reset;

		/// <summary>
		///     The loaded event is triggered after the file was changed or reload was called
		/// </summary>
		event EventHandler<IniSectionEventArgs> Saved;

		/// <summary>
		///     The loaded event is triggered after the file was changed or reload was called
		/// </summary>
		event EventHandler<IniSectionEventArgs> Saving;

		/// <summary>
		///     Try to get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues
		///     result
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <param name="value">out IniValue</param>
		/// <returns>bool with true if found</returns>
		bool TryGetIniValue(string propertyName, out IniValue value);
	}

	/// <summary>
	///     Generic version of IIniSection
	/// </summary>
	public interface IIniSection<T> : IIniSection
	{
		/// <summary>
		///     Get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues result
		/// </summary>
		/// <typeparam name="TProp">Your interface</typeparam>
		/// <param name="propertyExpression">expression for the property name</param>
		/// <returns>IniValue</returns>
		IniValue GetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		///     Try to get the IniValue for a property, this is quicker and uses less memory than to iterate over the GetIniValues
		///     result
		/// </summary>
		/// <typeparam name="TProp">Your interface</typeparam>
		/// <param name="propertyExpression">expression for the property name</param>
		/// <param name="value">out IniValue</param>
		/// <returns>bool with true if found</returns>
		bool TryGetIniValue<TProp>(Expression<Func<T, TProp>> propertyExpression, out IniValue value);
	}
}