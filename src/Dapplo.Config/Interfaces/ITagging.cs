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
using System.Linq.Expressions;

#endregion

namespace Dapplo.Config.Interfaces
{

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
}