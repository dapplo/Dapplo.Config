/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
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
	/// Extend your property interface with this, and all default values specified with the DefaultValueAttribute will be applied
	/// </summary>
	public interface IDefaultValue
	{
		/// <summary>
		/// Return the default value of the property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>the default value, null if none</returns>
		object DefaultValueFor(string propertyName);

		/// <summary>
		/// Restore the property value back to its default
		/// </summary>
		/// <param name="propertyName"></param>
		void RestoreToDefault(string propertyName);
	}

	/// <summary>
	/// Extend your property interface with this, and all default values specified with the DefaultValueAttribute will be applied
	/// </summary>
	public interface IDefaultValue<T> : IDefaultValue
	{
		/// <summary>
		/// Return the default value of the property
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		/// <returns>the default value, null if none</returns>
		object DefaultValueFor<TProp>(Expression<Func<T, TProp>> propertyExpression);

		/// <summary>
		/// Restore the property value back to its default
		/// </summary>
		/// <typeparam name="TProp"></typeparam>
		/// <param name="propertyExpression"></param>
		void RestoreToDefault<TProp>(Expression<Func<T, TProp>> propertyExpression);
	}
}