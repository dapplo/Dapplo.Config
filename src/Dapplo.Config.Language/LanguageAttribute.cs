// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dapplo.Config.Language
{
	/// <summary>
	///     Use this attribute to mark a language object, and the prefix
	/// </summary>
	[AttributeUsage(AttributeTargets.Interface)]
	public class LanguageAttribute : Attribute
	{
		/// <summary>
		///     Constructor
		/// </summary>
		/// <param name="prefix">string with the prefix</param>
		public LanguageAttribute(string prefix)
		{
			Prefix = prefix;
		}

		/// <summary>
		///     Name of the section in the ini file
		/// </summary>
		public string Prefix { get; }
	}
}