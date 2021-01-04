// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Dapplo.Config.Ini
{
	/// <summary>
	///     By making your property proxy interface extend this, you will be able to write the property to an ini file
	/// </summary>
	public interface IIniSectionInternal
    {
	    /// <summary>
        ///     This is called after the loading of the IniSection is finished and can be used to modify certain values before they are being used.
        /// </summary>
        Action<IIniSection> OnAfterLoad { get; }

        /// <summary>
        ///     This is called after the saving of the IniSection is finished and can be used to modify certain values
        /// </summary>
        Action<IIniSection> OnAfterSave { get; }

        /// <summary>
        ///     This is called before the saving of the IniSection is started and can be used to modify certain values
        /// </summary>
        Action<IIniSection> OnBeforeSave { get; }
    }
}