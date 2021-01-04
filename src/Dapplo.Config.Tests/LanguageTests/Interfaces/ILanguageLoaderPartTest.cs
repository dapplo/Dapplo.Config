// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;

namespace Dapplo.Config.Tests.LanguageTests.Interfaces
{
	public interface ILanguageLoaderPartTest
	{
		[DefaultValue(LanguageContainerTest.Ok)]
		string Ok2 { get; }
	}
}