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

#region using

using System;

#endregion

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