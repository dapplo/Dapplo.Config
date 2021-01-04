//  Dapplo // Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dapplo.Config.Ini
{
	/// <summary>
	///     Use this attribute on Properties where you want to influence the ini config behavior.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class IniPropertyBehaviorAttribute : Attribute
	{
		private bool? _ignoreErrors;

		/// <summary>
		///     Constructor
		/// </summary>
		public IniPropertyBehaviorAttribute()
		{
			Read = true;
			Write = true;
		}

		/// <summary>
		///     Set ignore errors to false, if you want an exception when a parse error occurs.
		///     Default this is set to true, which will cause the property to have the "default" value.
		/// </summary>
		public bool IgnoreErrors
		{
			get { return _ignoreErrors ?? true; }
			set { _ignoreErrors = value; }
		}

		/// <summary>
		///     Specifies if the IgnoreErrors was specified or is default
		/// </summary>
		public bool IsIgnoreErrorsSet => _ignoreErrors.HasValue;

		/// <summary>
		///     Default is true, set read to false to skip reading.
		///     Although this might be unlikely, examples are:
		///     1 A property with the last changed date, which might not be the date of the file
		///     2 A property with the application or component version which "processed" the configuration
		/// </summary>
		public bool Read { get; set; }

		/// <summary>
		///     Default is true, set write to false to skip serializing.
		/// </summary>
		public bool Write { get; set; }
	}
}