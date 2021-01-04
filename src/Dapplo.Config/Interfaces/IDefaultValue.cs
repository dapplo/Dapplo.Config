// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    ///     Extend your property interface with this, and all default values specified with the DefaultValueAttribute will be
    ///     applied
    /// </summary>
    public interface IDefaultValue
	{
		/// <summary>
		///     Return the default value of the property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>the default value, null if none</returns>
		object DefaultValueFor(string propertyName);

		/// <summary>
		///     Restore the property value back to its default
		/// </summary>
		/// <param name="propertyName"></param>
		void RestoreToDefault(string propertyName);
    }
}