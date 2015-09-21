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
using System.Text.RegularExpressions;

namespace Dapplo.Config.Support
{
	public class GetSetInfo
	{
		private static readonly Regex _propertyCleanup = new Regex(@"[^a-zA-Z0-9]+", RegexOptions.Compiled);

		/// <summary>
		/// Make a clean & "tolower" string of a property name, this will be used internally
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static string CleanupPropertyName(string propertyName)
		{
			if (propertyName == null)
			{
				return null;
			}
			return _propertyCleanup.Replace(propertyName, "").ToLowerInvariant();
        }

		private string _propertyName;
		/// <summary>
		///     Property name of the property that is being get/set
		/// </summary>
		public string PropertyName
		{
			get
			{
				return _propertyName;
            }
			set
			{
				_propertyName = value;
				CleanedPropertyName = CleanupPropertyName(value);
            }
		}

		/// <summary>
		/// The property name of the property that is being get/set, but tolower and without special characters
		/// </summary>
		public string CleanedPropertyName
		{
			get;
			private set;
		}

		/// <summary>
		///     This can be set to an exception if a getter/setter wants to throw an exception
		/// </summary>
		public Exception Error
		{
			get;
			set;
		}

		/// <summary>
		/// Simple property to check for error
		/// </summary>
		public bool HasError
		{
			get
			{
				return Error != null;
			}
		}

		/// <summary>
		///     Can the proxy continue with other getter/setters?
		///     This should be set to false if a getter/setter implementation wants to throw an exception or thinks there should be
		///     no more others.
		/// </summary>
		public bool CanContinue
		{
			get;
			set;
		}
	}
}