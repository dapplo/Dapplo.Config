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

using System;

namespace Dapplo.Config.Ini
{
	/// <summary>
	/// Use this attribute on Properties where you want to influence the ini config behavior.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class IniPropertyBehaviorAttribute : Attribute
	{
		private bool? _ignoreErrors;

		public IniPropertyBehaviorAttribute()
		{
			Read = true;
			Write = true;
		}

		/// <summary>
		/// Default is true, set read to false to skip reading.
		/// Although this might be unlikely, examples are:
		/// 1 A property with the last changed date, which might not be the date of the file
		/// 2 A property with the application or component version which "processed" the configuration
		/// </summary>
		public bool Read
		{
			get;
			set;
		}

		/// <summary>
		/// Default is true, set write to false to skip serializing.
		/// </summary>
		public bool Write
		{
			get;
			set;
		}

		/// <summary>
		/// Specifies if the IgnoreErrors was specified or is default
		/// </summary>
		public bool IsIgnoreErrorsSet
		{
			get
			{
				return _ignoreErrors.HasValue;
            }
		}

		/// <summary>
		/// Set ignore errors to false, if you want an exception when a parse error occurs.
		/// Default this is set to true, which will cause the property to have the "default" value.
		/// </summary>
		public bool IgnoreErrors
		{
			get
			{
				return _ignoreErrors.HasValue?_ignoreErrors.Value:true;
			}
			set
			{
				_ignoreErrors = value;
            }
		}
	}
}