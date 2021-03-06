﻿// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	public interface ITransactionTest : ITransactionalProperties
	{
		int Age { get; set; }
	}
}