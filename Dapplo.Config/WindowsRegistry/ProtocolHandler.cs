/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Config

	Dapplo.Config is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Config is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have Config a copy of the GNU Lesser General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.
 */

using Microsoft.Win32;

namespace Dapplo.Config.WindowsRegistry
{
	public static class ProtocolHandler
	{
		/// <summary>
		/// Registers an user defined URL protocol for the usage with
		/// the Windows Shell, the Internet Explorer and Office.
		/// 
		/// Example for an URL of an user defined URL protocol:
		/// 
		///   rainbird://RemoteControl/OpenFridge/GetBeer
		/// </summary>
		/// <param name="protocolName">Name of the protocol (e.g. "rainbird" für "rainbird://...")</param>
		/// <param name="applicationPath">Complete file system path to the EXE file, which processes the URL being called (the complete URL is handed over as a Command Line Parameter).</param>
		/// <param name="description">Description (e.g. "URL:Rainbird Custom URL")</param>
		public static void RegisterUrlProtocol(string protocolName, string applicationPath, string description)
		{
			// Register at CurrentUser\Software\Classes
			using (var softwareClassesKey = Registry.CurrentUser.OpenSubKey(@"Software\Classes", true))
			{
				// Apply protocol name
				if (softwareClassesKey == null)
				{
					return;
				}
				using (var protocolKey = softwareClassesKey.CreateSubKey(protocolName))
				{
					// Assign protocol
					if (protocolKey == null)
					{
						return;
					}
					protocolKey.SetValue(null, description);
					protocolKey.SetValue("URL Protocol", string.Empty);
					// Create Shell
					using (var shellKey = protocolKey.CreateSubKey("Shell"))
					{
						// Create open
						if (shellKey == null)
						{
							return;
						}
						using (var openKey = shellKey.CreateSubKey("open"))
						{
							// Create command
							if (openKey == null)
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
