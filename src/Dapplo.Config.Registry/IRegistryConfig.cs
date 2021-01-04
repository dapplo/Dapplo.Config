// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Registry
{
    /// <summary>
    ///     Interface for a registry object
    /// </summary>
    public interface IRegistry
	{
		/// <summary>
		///     The path for the property
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>string with the path</returns>
		string PathFor(string propertyName);
	}
}