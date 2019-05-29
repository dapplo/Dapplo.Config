//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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

using System.Collections.Generic;

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    ///     Extending the to be property interface with this, adds a way of know if there were changes sind the last reset
    ///     Is used internally in the IniConfig to detect if a write is needed
    /// </summary>
    public interface IHasChanges
	{
        /// <summary>
        /// This can be used to turn on change tracking
        /// </summary>
        void TrackChanges();

        /// <summary>
        /// This can be used to stop change tracking
        /// </summary>
        void DoNotTrackChanges();

        /// <summary>
        ///     Check if there are changes pending
        /// </summary>
        /// <returns>true when there are changes</returns>
        bool HasChanges();

		/// <summary>
		///     Reset the has changes flag
		/// </summary>
		void ResetHasChanges();

		/// <summary>
		/// Retrieve all changes, 
		/// </summary>
		/// <returns>ISet with the property values</returns>
		ISet<string> Changes();

		/// <summary>
		/// Test if a property has been changed since the last reset
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>bool</returns>
		bool IsChanged(string propertyName);
	}
}