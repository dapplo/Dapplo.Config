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

namespace Dapplo.Config.Interfaces
{
	/// <summary>
	///     Extending the to be property interface with this, adds write protection
	/// </summary>
	public interface IWriteProtectProperties
	{
		/// <summary>
		///     Disable the write protection of the supplied property
		/// </summary>
		/// <param name="propertyName">Name of the property to disable the write protect for</param>
		void DisableWriteProtect(string propertyName);

		/// <summary>
		///     Test if the supplied property is write protected
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>true if the property is protected</returns>
		bool IsWriteProtected(string propertyName);

		/// <summary>
		///     Remove the write protection
		/// </summary>
		void RemoveWriteProtection();

		/// <summary>
		///     After calling this method, every changed property will be write-protected
		/// </summary>
		void StartWriteProtecting();

		/// <summary>
		///     End the write protecting
		/// </summary>
		void StopWriteProtecting();

		/// <summary>
		///     Write protect the supplied property
		/// </summary>
		/// <param name="propertyName">Name of the property to write protect</param>
		void WriteProtect(string propertyName);
	}

	/// <summary>
	///     Extending the to be property interface with this, adds write protection
	/// </summary>
	public interface IWriteProtectProperties<T> : IWriteProtectProperties
	{
		/// <summary>
		///     Disable the write protection of the supplied property (by lamdba)
		/// </summary>
		/// <typeparam name="TProp">will be automatically set by the expression</typeparam>
		/// <param name="propertyExpression">Property to disable the write protect</param>
		void DisableWriteProtect<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		///     Test if the supplied property (by lamdba) is write protected
		/// </summary>
		/// <typeparam name="TProp">will be automatically set by the expression</typeparam>
		/// <param name="propertyExpression">Property to test</param>
		/// <returns>true if the property is protected</returns>
		bool IsWriteProtected<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		///     Write protect the supplied property (by lamdba)
		/// </summary>
		/// <typeparam name="TProp">will be automatically set by the expression</typeparam>
		/// <param name="propertyExpression">Property to write protect</param>
		void WriteProtect<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}
}