// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    ///     Extend your property interface with this, and you can read the DescriptionAttribute
    /// </summary>
    public interface IDescription
	{
		/// <summary>
		///     Return the description of the property
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns>the description, null if none</returns>
		string DescriptionFor(string propertyName);
	}
}