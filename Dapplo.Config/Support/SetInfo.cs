/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2014 Robin Krom
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

namespace Dapplo.Config.Support {
	/// <summary>
	///     This class contains all the information for the setter actions
	/// </summary>
	public class SetInfo : GetSetInfo {
		/// <summary>
		///     The new value for the property
		/// </summary>
		public object NewValue {
			get;
			set;
		}

		/// <summary>
		///     The old value of the property, if any (see HasOldValue)
		/// </summary>
		public object OldValue {
			get;
			set;
		}

		/// <summary>
		///     Does property have an old value?
		/// </summary>
		public bool HasOldValue {
			get;
			set;
		}
	}
}