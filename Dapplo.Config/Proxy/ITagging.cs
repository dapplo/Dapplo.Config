//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using System.Linq.Expressions;

#endregion

namespace Dapplo.Config.Proxy
{
	/// <summary>
	///     Attribute to "Tag" properties as with certain information
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TagAttribute : Attribute
	{
		public TagAttribute(object tag)
		{
			Tag = tag;
		}

		public TagAttribute(object tag, object tagValue) : this(tag)
		{
			TagValue = tagValue;
		}

		public object Tag { get; set; }

		public object TagValue { get; set; }
	}

	/// <summary>
	///     Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	public interface ITagging
	{
		/// <summary>
		///     Retrieve the value for tag
		/// </summary>
		/// <param name="propertyName">Name of the property to get the tag value</param>
		/// <param name="tag">The tag value to get</param>
		/// <returns>Tagged value or null</returns>
		object GetTagValue(string propertyName, object tag);

		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith(string propertyName, object tag);
	}

	/// <summary>
	///     Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ITagging<T> : ITagging
	{
		/// <summary>
		///     Retrieve the value for tag
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>Tagged value or null</returns>
		object GetTagValue<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);

		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);
	}
}