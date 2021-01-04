// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Ini.Rest
{
    /// <summary>
    ///     The supported commands for the ini-REST api
    /// </summary>
    public enum IniRestCommands
    {
        /// <summary>
        ///     Set a value
        /// </summary>
        Set,

        /// <summary>
        ///     Get a value
        /// </summary>
        Get,

        /// <summary>
        ///     Reset a value (to it's default)
        /// </summary>
        Reset,

        /// <summary>
        ///     Add a value to a collection
        /// </summary>
        Add,

        /// <summary>
        ///     Remove a value from a collection
        /// </summary>
        Remove
    }
}