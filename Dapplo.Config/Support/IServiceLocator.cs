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
using System.Collections.Generic;

namespace Dapplo.Config.Support
{
	public interface IServiceLocator
	{
		/// <summary>
		/// Export an object, without using Attribute
		/// </summary>
		void Export<T>(T obj);

		/// <summary>
		/// Export an object, without using Attribute
		/// </summary>
		void Export<T>(string contractName, T obj);

		/// <summary>
		/// Fill all the imports in the object isntance
		/// </summary>
		/// <param name="importingObject">object to fill the imports for</param>
		void FillImports(object importingObject);

		/// <summary>
		/// Simple "service-locater"
		/// </summary>
		/// <typeparam name="T">Type to locate</typeparam>
		/// <returns>Lazy T</returns>
		Lazy<T> GetExport<T>();

		/// <summary>
		/// Simple "service-locater" with meta-data
		/// </summary>
		/// <typeparam name="T">Type to locate</typeparam>
		/// <typeparam name="TMetaData">Type for the meta-data</typeparam>
		/// <returns>Lazy T,TMetaData</returns>
		Lazy<T, TMetaData> GetExport<T, TMetaData>();

		/// <summary>
		/// Simple "service-locater" to get multiple exports
		/// </summary>
		/// <typeparam name="T">Type to locate</typeparam>
		/// <returns>IEnumerable of Lazy T</returns>
		IEnumerable<Lazy<T>> GetExports<T>();

		/// <summary>
		/// Simple "service-locater" to get multiple exports with meta-data
		/// </summary>
		/// <typeparam name="T">Type to locate</typeparam>
		/// <typeparam name="TMetaData">Type for the meta-data</typeparam>
		/// <returns>IEnumerable of Lazy T,TMetaData</returns>
		IEnumerable<Lazy<T, TMetaData>> GetExports<T, TMetaData>();
	}
}
