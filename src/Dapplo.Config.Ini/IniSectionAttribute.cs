// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dapplo.Config.Ini
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
		public string Name { get; }
	}
}