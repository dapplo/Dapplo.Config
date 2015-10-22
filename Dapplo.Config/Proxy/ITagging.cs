/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Linq.Expressions;

namespace Dapplo.Config.Proxy
{
	/// <summary>
	///	 Attribute to "Tag" properties as with certain information
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class TagAttribute : Attribute
	{
		public object Tag
		{
			get;
			set;
		}

		public object TagValue
		{
			get;
			set;
		}

		public TagAttribute(object tag)
		{
			Tag = tag;
		}

		public TagAttribute(object tag, object tagValue) : this(tag) {
			TagValue = tagValue;
		}
	}

	/// <summary>
	/// Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	public interface ITagging
	{
		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith(string propertyName, object tag);

		/// <summary>
		/// Retrieve the value for tag
		/// </summary>
		/// <param name="propertyName">Name of the property to get the tag value</param>
		/// <param name="tag">The tag value to get</param>
		/// <returns>Tagged value or null</returns>
		object GetTagValue(string propertyName, object tag);
	}

	/// <summary>
	/// Interface which your interface needs to implement to be able to see if a property is tagged
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface ITagging<T> : ITagging
	{
		/// <summary>
		///     Checks if the supplied expression resolves to a property which has the expert attribute
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>true if the property has the expert attribute, else false</returns>
		bool IsTaggedWith<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);

		/// <summary>
		/// Retrieve the value for tag
		/// </summary>
		/// <typeparam name="TProp">Your interfaces</typeparam>
		/// <param name="propertyExpression"></param>
		/// <param name="tag">Tag to check if the property is tagged with</param>
		/// <returns>Tagged value or null</returns>
		object GetTagValue<TProp>(Expression<Func<T, TProp>> propertyExpression, object tag);
	}
}