//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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

using System;
using Dapplo.Config.Extensions;

namespace Dapplo.Config
{
	/// <summary>
	///     A StringComparer which ignores everything which is not a letter
	/// </summary>
	public class AbcComparer : StringComparer
	{
	    /// <summary>
	    /// A already provided AbcComparer instance
	    /// </summary>
	    public static AbcComparer Instance { get; } = new AbcComparer();

        /// <summary>
        /// Implement the StringComparer.Compare
        /// </summary>
        /// <param name="x">string</param>
        /// <param name="y">string</param>
        /// <returns>
        /// A signed integer that indicates the relative values of x and y, as shown in the following table.
        /// Less than zero: x precedes y in the sort order -or-x is null and y is not null.
        /// Zero: x is equal to y -or-x and y are both null.
        /// Greater than zero: x follows y in the sort order -or- y is null and x is not null.</returns>
        public override int Compare(string x, string y)
		{
			if (x == null && y != null)
			{
				return -1;
			}

			if (x != null && y == null)
			{
				return 1;
			}

			return x == null ? 0 : string.Compare(x.RemoveNonAlphaDigitsToLower(), y.RemoveNonAlphaDigitsToLower(), StringComparison.Ordinal);
		}

		/// <summary>
		/// Check if values are equal
		/// </summary>
		/// <param name="x">string</param>
		/// <param name="y">string</param>
		/// <returns>true if x and y are the same</returns>
		public override bool Equals(string x, string y)
		{
			if (x == null && y != null)
			{
				return false;
			}

			if (x != null && y == null)
			{
				return false;
			}

			return x == null || x.RemoveNonAlphaDigitsToLower().Equals(y.RemoveNonAlphaDigitsToLower());
		}

		/// <summary>
		/// Returns the hashcode of the passed string after it was modified by removing all non digits or alphas, and running tolower.
		/// </summary>
		/// <param name="obj">string</param>
		/// <returns>int with hashcode</returns>
		public override int GetHashCode(string obj)
		{
			return obj.RemoveNonAlphaDigitsToLower().GetHashCode();
		}
	}
}