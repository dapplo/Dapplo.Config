/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
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

using System;

namespace Dapplo.Config.Ini
{
	[AttributeUsage(AttributeTargets.Interface)]
	public class IniSectionAttribute : Attribute
	{
		public IniSectionAttribute(string name)
		{
			Name = name;
			IgnoreErrors = true;
		}

		/// <summary>
		/// Name of the section in the ini file
		/// </summary>
		public string Name
		{
			get;
			private set;
		}

		/// <summary>
		/// Set ignore errors to false, if you want an exception when a parse error occurs.
		/// Default this is set to true, which will cause the property to have the "default" value.
		/// </summary>
		public bool IgnoreErrors
		{
			get;
			set;
		}
	}
}