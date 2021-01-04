// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Language;

namespace Dapplo.Config.Tests.LanguageTests.Interfaces
{
	[Language("TestFail")]
	public interface ILanguageLoaderFailTest : ILanguage
	{
		[DefaultValue(LanguageContainerTest.Ok)]
		string Ok { get;
			// Set is not allowed!
			set; }
	}
}