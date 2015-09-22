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

using System.Text.RegularExpressions;

namespace Dapplo.Config.Support
{
	public static class StringExtensions
	{
		private static readonly Regex _cleanup = new Regex(@"[^a-z0-9]+", RegexOptions.Compiled);
		/// <summary>
		/// Helper method for converting a string to a non strict value.
		/// This means, ToLowerInvariant and replace all non alpha/digits to ""
		/// </summary>
		/// <param name="key"></param>
		/// <returns>clean string</returns>
		public static string Cleanup(this string value)
		{
			return _cleanup.Replace(value.ToLowerInvariant(), "");
		}

		public static bool NonStrictEquals(this string value1, string value2)
		{
			return value1.Cleanup().Equals(value2.Cleanup());
        }
	}
}
