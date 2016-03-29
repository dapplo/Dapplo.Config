﻿//  Dapplo - building blocks for desktop applications
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
using System.Linq;
using System.Reflection;

#endregion

namespace Dapplo.Config.Interceptor.Extensions
{
	/// <summary>
	///     Base class for extensions, this should take away some default handling
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class AbstractInterceptorExtension<T> : IInterceptorExtension
	{
		/// <summary>
		/// The intercepting instance
		/// </summary>
		public IExtensibleInterceptor Interceptor { get; set; }

		/// <summary>
		/// Initialize the extension, e.g. register methods etc.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		///     This returns 0, which means somewhere in the middle
		///     If an extension needs to be called last, it should override this and for example return int.MaxValue
		///     If an extension needs to be called first, it should override this and for example return int.MinValue
		/// </summary>
		public virtual int InitOrder => 0;

		/// <summary>
		///     Handle every property
		/// </summary>
		/// <param name="propertyInfo"></param>
		public virtual void InitProperty(PropertyInfo propertyInfo)
		{
		}

		/// <summary>
		///     After property initialization
		/// </summary>
		public virtual void AfterInitialization()
		{
		}
	}
}