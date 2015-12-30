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

namespace Dapplo.Config.Proxy
{
	/// <summary>
	///     Extending the to be property interface with this, adds a way of know if there were changes sind the last reset
	///     Is used internally in the IniConfig to detect if a write is needed
	/// </summary>
	public interface IHasChanges
	{
		/// <summary>
		/// Reset the has changes flag
		/// </summary>
		void ResetHasChanges();

		/// <summary>
		/// Check if there are changes pending
		/// </summary>
		/// <returns>true when there are changes</returns>
		bool HasChanges();
	}
}