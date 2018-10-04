//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
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

namespace Dapplo.Config.Attributes
{
    /// <summary>
    ///     Attribute to "Tag" properties as with certain information
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TagAttribute : Attribute
	{
		/// <summary>
		/// Constructor for the TagAttribute
		/// </summary>
		/// <param name="tag">object with value for the tag</param>
		public TagAttribute(object tag)
		{
			Tag = tag;
		}

		/// <summary>
		/// Constructor for the TagAttribute
		/// </summary>
		/// <param name="tag">object with value for the tag</param>
		/// <param name="tagValue">object with value for the tag value</param>
		public TagAttribute(object tag, object tagValue) : this(tag)
		{
			TagValue = tagValue;
		}

		/// <summary>
		/// The tag
		/// </summary>
		public object Tag { get; set; }

		/// <summary>
		/// Get (or set) the value of the tag
		/// </summary>
		public object TagValue { get; set; }
	}
}