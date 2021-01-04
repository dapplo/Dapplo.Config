// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dapplo.Config.Registry
{
	/// <summary>
	///     This is the handler for a custom protocol
	/// </summary>
	public static class ProtocolHandler
	{
		/// <summary>
		///     Registers an user defined URL protocol for the usage with
		///     the Windows Shell, the Internet Explorer and Office.
		///     Example for an URL of an user defined URL protocol:
		///     rainbird://RemoteControl/OpenFridge/GetBeer
		/// </summary>
		/// <param name="protocolName">Name of the protocol (e.g. "rainbird" für "rainbird://...")</param>
		/// <param name="applicationPath">
		///     Complete file system path to the EXE file, which processes the URL being called (the
		///     complete URL is handed over as a Command Line Parameter).
		/// </param>
		/// <param name="description">Description (e.g. "URL:Rainbird Custom URL")</param>
		public static void RegisterUrlProtocol(string protocolName, string applicationPath, string description)
		{
			// Register at CurrentUser\Software\Classes
			using (var softwareClassesKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
			{
				// Apply protocol name
				if (softwareClassesKey is null)
				{
					return;
				}
				using (var protocolKey = softwareClassesKey.CreateSubKey(protocolName))
				{
					// Assign protocol
					if (protocolKey is null)
					{
						return;
					}
					protocolKey.SetValue(null, description);
					protocolKey.SetValue("URL Protocol", string.Empty);
					// Create Shell
					using (var shellKey = protocolKey.CreateSubKey("Shell"))
					{
						// Create open
						if (shellKey is null)
						{
							return;
						}
						using (var openKey = shellKey.CreateSubKey("open"))
						{
							// Create command
							if (openKey is null)
							{
								return;
							}
							using (var commandKey = openKey.CreateSubKey("command"))
							{
								// Specify application handling the URL protocol
								commandKey?.SetValue(null, "\"" + applicationPath + "\" %1");
							}
						}
					}
				}
			}
		}
	}
}