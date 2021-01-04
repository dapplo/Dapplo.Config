//  Da// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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