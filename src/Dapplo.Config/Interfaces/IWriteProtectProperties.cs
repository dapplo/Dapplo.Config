// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Interfaces
{
    /// <summary>
    ///     Extending the to be property interface with this, adds write protection
    /// </summary>
    public interface IWriteProtectProperties
	{
		/// <summary>
		///     Disable the write protection of the supplied property
		/// </summary>
		/// <param name="propertyName">Name of the property to disable the write protect for</param>
		void DisableWriteProtect(string propertyName);

		/// <summary>
		///     Test if the supplied property is write protected
		/// </summary>
		/// <param name="propertyName">Name of the property</param>
		/// <returns>true if the property is protected</returns>
		bool IsWriteProtected(string propertyName);

		/// <summary>
		///     Remove the write protection
		/// </summary>
		void RemoveWriteProtection();

		/// <summary>
		///     After calling this method, every changed property will be write-protected
		/// </summary>
		void StartWriteProtecting();

		/// <summary>
		///     End the write protecting
		/// </summary>
		void StopWriteProtecting();

		/// <summary>
		///     Write protect the supplied property
		/// </summary>
		/// <param name="propertyName">Name of the property to write protect</param>
		void WriteProtect(string propertyName);
	}
}