// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Ini;

namespace Dapplo.Config.Tests.IniTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	[IniSection("Test")]
	[Description("Test Configuration")]
	public interface IIniConfigWrongEnumTest : IIniConfigSubInterfaceTest, IIniSection
	{
		[Description("Test property for wrong enums")]
		[DefaultValue("Value3")]
		IniConfigTestValues TestWithFalseEnum { get; set; }
	}
}