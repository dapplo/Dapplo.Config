// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Dapplo.Config.Extensions
{
	/// <summary>
	///     Extensions for enums
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		///     The returns the Value from the EnumMemberAttribute, or a ToString on the element.
		///     This can be used to create a lookup from string to enum element
		/// </summary>
		/// <param name="enumerationItem">Enum</param>
		/// <returns>string</returns>
		public static string EnumValueOf(this Enum enumerationItem)
		{
			if (enumerationItem == null)
			{
				return null;
			}

			var enumString = enumerationItem.ToString();
			var attribute = enumerationItem.GetType().GetRuntimeField(enumString)?.GetAttribute<EnumMemberAttribute>(false);
			return attribute != null ? attribute.Value : enumString;
		}
	}
}