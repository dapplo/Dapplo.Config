// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	public interface IDefaultValueWithErrorTest : IDefaultValue
	{
		[Description("Test property for enums"), DefaultValue("Value3")]
		SimpleEnum MyEnum { get; set; }
	}

	public enum SimpleEnum
	{
		Value1,
		Value2
	}
}