// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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