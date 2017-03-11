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

#endregion

namespace Dapplo.Ini
{
	/// <summary>
	///     This attribute should be used to mark a class as IniSection
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public class IniSectionAttribute : Attribute
	{
		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="name">Name of the ini-section</param>
		public IniSectionAttribute(string name)
		{
			Name = name;
			IgnoreErrors = true;
		}

		/// <summary>
		///     Set ignore errors to false, if you want an exception when a parse error occurs.
		///     Default this is set to true, which will cause the property to have the "default" value.
		/// </summary>
		public bool IgnoreErrors { get; set; }

		/// <summary>
		///     Name of the section in the ini file
		/// </summary>
		public string Name { get; private set; }
	}
}