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

using System.Reflection;

#endregion

namespace Dapplo.Config.Interceptor
{
	/// <summary>
	///     Extensions for the IExtensibleInterceptor need to extend this interface.
	/// </summary>
	public interface IInterceptorExtension
	{
		/// <summary>
		/// Type which intercepts
		/// </summary>
		IExtensibleInterceptor Interceptor { get; set; }

		/// <summary>
		/// Initialize the extension, this should register the methods/get/set
		/// </summary>
		void Initialize();

		/// <summary>
		///     Specify the Init-Order, low first and high later
		/// </summary>
		int InitOrder { get; }

		/// <summary>
		///     After property initialization, in here exceptions can be ignored or caches created
		/// </summary>
		void AfterInitialization();

		/// <summary>
		///     This is called for every Property on type T, so we only have 1x reflection
		/// </summary>
		/// <param name="propertyInfo"></param>
		void InitProperty(PropertyInfo propertyInfo);
	}
}