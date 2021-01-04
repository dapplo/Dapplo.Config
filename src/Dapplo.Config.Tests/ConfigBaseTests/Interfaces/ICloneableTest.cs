// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Interfaces
{
	public interface ICloneableTest : IShallowCloneable
	{
		string Name { get; set; }
	}
}