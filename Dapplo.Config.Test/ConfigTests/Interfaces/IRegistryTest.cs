﻿/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using Dapplo.Config.WindowsRegistry;
using Microsoft.Win32;

namespace Dapplo.Config.Test.ConfigTests.Interfaces
{
	/// <summary>
	/// This is the interface under test
	/// </summary>
	[Registry(@"Software\Microsoft\Windows\CurrentVersion", Hive = RegistryHive.CurrentUser, View = RegistryView.Registry32)]
	public interface IRegistryTest : IRegistry
	{
		[RegistryProperty(@"Run", Hive = RegistryHive.LocalMachine)]
		Dictionary<string, object> LmRun32
		{
			get;
			set;
		}

		[RegistryProperty(@"Run", Hive = RegistryHive.LocalMachine, View = RegistryView.Registry64)]
		Dictionary<string, object> LmRun64
		{
			get;
			set;
		}

		[RegistryProperty(@"Run")]
		Dictionary<string, object> CuRun32
		{
			get;
			set;
		}

		[RegistryProperty(@"Run", View = RegistryView.Registry64)]
		Dictionary<string, object> CuRun64
		{
			get;
			set;
		}

		[RegistryProperty(@"\Software\Microsoft\Windows NT\CurrentVersion", "ProductName", Hive = RegistryHive.LocalMachine, View = RegistryView.Default)]
		string ProductName
		{
			get;
			set;
		}
	}
}