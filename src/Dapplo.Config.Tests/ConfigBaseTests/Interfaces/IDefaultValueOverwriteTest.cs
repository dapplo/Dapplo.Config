// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	public interface IDefaultValueOverwriteTest : IDefaultValueBase, IDefaultValue
    {
		[DefaultValue(42)]
		new int Age { get; set; }
	}
}