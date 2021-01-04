// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

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