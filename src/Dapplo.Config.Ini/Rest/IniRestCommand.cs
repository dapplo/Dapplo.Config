// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Dapplo.Config.Ini.Rest
{
	/// <summary>
	///     This is a container for the IniRest, it has all the needed information to process
	/// </summary>
	public class IniRestCommand
	{
		/// <summary>
		///     The application for this rest command
		/// </summary>
		public string Application { get; set; }

		/// <summary>
		///     The command to process
		/// </summary>
		public IniRestCommands Command { get; set; }

		/// <summary>
		///     The ini file for this rest command
		/// </summary>
		public string File { get; set; }

		/// <summary>
		///     The IniValues that were specified for the get/set/reset or add/remove
		/// </summary>
		public IList<IniValue> Results { get; set; } = new List<IniValue>();

		/// <summary>
		///     The ini-section for this rest command
		/// </summary>
		public string Section { get; set; }

		/// <summary>
		///     For add / remove we NEED a target, for set/reset it's possible to specify one
		/// </summary>
		public string Target { get; set; }

		/// <summary>
		///     These values are only keys when get, reset or remove, key/values when set or add
		/// </summary>
		public IDictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
	}
}