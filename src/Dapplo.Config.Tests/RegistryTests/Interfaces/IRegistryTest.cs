// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Dapplo.Config.Registry;
using Microsoft.Win32;

namespace Dapplo.Config.Tests.RegistryTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	[Registry(@"Software\Microsoft\Windows\CurrentVersion", Hive = RegistryHive.CurrentUser, View = RegistryView.Registry32)]
	public interface IRegistryTest : IRegistry
	{
		[Registry(@"Run")]
		IDictionary<string, object> CuRun32 { get; set; }

		[Registry(@"Run", View = RegistryView.Registry64)]
		IDictionary<string, object> CuRun64 { get; set; }

		[Registry(@"Run", Hive = RegistryHive.LocalMachine)]
		IDictionary<string, object> LmRun32 { get; set; }

		[Registry(@"Run", Hive = RegistryHive.LocalMachine, View = RegistryView.Registry64)]
		IDictionary<string, object> LmRun64 { get; set; }

		[Registry(@"\Software\Microsoft\Windows NT\CurrentVersion", "ProductName", IgnoreBasePath = true, Hive = RegistryHive.LocalMachine, View = RegistryView.Default)]
		string ProductName { get; set; }
	}
}