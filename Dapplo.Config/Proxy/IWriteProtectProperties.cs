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
	///     Extending the to be property interface with this, adds write protection
	/// </summary>
	public interface IWriteProtectProperties
	{
		/// <summary>
		/// After calling this method, every changed property will be write-protected
		/// </summary>
		void StartWriteProtecting();

		/// <summary>
		/// End the write protecting
		/// </summary>
		void StopWriteProtecting();

		/// <summary>
		/// Remove the write protection
		/// </summary>
		void RemoveWriteProtection();

		/// <summary>
		/// Test if the supplied property is write protected
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>true if the property is protected</returns>
		bool IsWriteProtected(string propertyName);

		/// <summary>
		/// Write protect the supplied property
		/// </summary>
		/// <param name="propertyName">Name of the property to write protect</param>
		void WriteProtect(string propertyName);

		/// <summary>
		/// Disable the write protection of the supplied property
		/// </summary>
		/// <param name="propertyName">Name of the property to disable the write protect for</param>
		void DisableWriteProtect(string propertyName);
	}

	/// <summary>
	///     Extending the to be property interface with this, adds write protection
	/// </summary>
	public interface IWriteProtectProperties<T> : IWriteProtectProperties
	{
		/// <summary>
		/// Test if the supplied property (by lamdba) is write protected
		/// </summary>
		/// <typeparam name="TProp">will be automatically set by the expression</typeparam>
		/// <param name="propertyExpression">Property to test</param>
		/// <returns>true if the property is protected</returns>
		bool IsWriteProtected<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		/// Write protect the supplied property (by lamdba)
		/// </summary>
		/// <typeparam name="TProp">will be automatically set by the expression</typeparam>
		/// <param name="propertyExpression">Property to write protect</param>
		void WriteProtect<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		/// Disable the write protection of the supplied property (by lamdba)
		/// </summary>
		/// <typeparam name="TProp">will be automatically set by the expression</typeparam>
		/// <param name="propertyExpression">Property to disable the write protect</param>
		void DisableWriteProtect<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}
}