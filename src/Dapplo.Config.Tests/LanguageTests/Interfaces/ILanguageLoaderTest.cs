// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Language;

namespace Dapplo.Config.Tests.LanguageTests.Interfaces
{
	[Language("Test")]
	public interface ILanguageLoaderTest : ILanguage, ILanguageLoaderPartTest
	{
		[DefaultValue(LanguageContainerTest.Ok)]
		string Ok { get; }

		string OnlydeDe { get; }

		string OnlyenUs { get; }

		string OnlynlNl { get; }

		string TestValue { get; }
	}
}