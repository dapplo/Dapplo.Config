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

using System.Reflection;

namespace Dapplo.Config
{
	/// <summary>
	///     Extensions need to extend this interface.
	/// </summary>
	public interface IPropertyProxyExtension
	{
		/// <summary>
		/// This is called for every Property on type T, so we only have 1x reflection
		/// </summary>
		/// <param name="propertyInfo"></param>
		void InitProperty(PropertyInfo propertyInfo);

		/// <summary>
		/// Specify the Init-Order, low first and high later 
		/// </summary>
		int InitOrder
		{
			get;
		}

		/// <summary>
		/// After property initialization, in here exceptions can be ignored or caches created
		/// </summary>
		void AfterInitialization();
	}
}