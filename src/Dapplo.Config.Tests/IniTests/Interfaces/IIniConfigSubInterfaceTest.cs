// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Dapplo.Config.Tests.IniTests.Interfaces
{
	public interface IIniConfigSubInterfaceTest
	{
		string SubValue { get; set; }

		[DefaultValue("It works!")]
		string SubValuewithDefault { get; set; }

		[Description("Test property 2 for enums")]
		[DefaultValue(IniConfigTestValues.Value2)]
		IniConfigTestValues TestWithEnumSubValue { get; set; }
	}
}