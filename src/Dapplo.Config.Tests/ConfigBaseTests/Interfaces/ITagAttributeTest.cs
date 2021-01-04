// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Interfaces;
using Dapplo.Config.Attributes;

namespace Dapplo.Config.Tests.ConfigBaseTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	public interface ITagAttributeTest : ITagging
	{
		[Tag("Expert")]
		int Age { get; set; }

		[Tag(TestTags.Tag2), Tag(TestTags.Tag1, "Robin")]
		string FirstName { get; set; }

		string Name { get; set; }
	}
}