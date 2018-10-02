﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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
using Dapplo.Config.Interfaces;

#endregion

namespace Dapplo.Registry
{
	/// <summary>
	///     Interface for a registry object
	/// </summary>
	public interface IRegistry : IDefaultValue, IWriteProtectProperties
	{
		/// <summary>
		///     The path for the property
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>string with the path</returns>
		string PathFor(string propertyName);
	}

	/// <summary>
	///     Generic version of the IRegistry interface
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IRegistry<T> : IRegistry, IDefaultValue<T>, IWriteProtectProperties<T>
	{
		/// <summary>
		///     Return the registry path for a property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>the path to a property</returns>
		string PathFor<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}
}