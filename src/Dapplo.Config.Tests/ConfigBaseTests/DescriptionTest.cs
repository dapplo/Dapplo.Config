// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	public class DescriptionTest
	{
		public const string TestDescription = "Name of the person";
		private readonly IDescriptionTest _descriptionTest;

		public DescriptionTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            _descriptionTest = DictionaryConfiguration<IDescriptionTest>.Create();
        }

		[Fact]
		public void TestDescriptionAttribute()
		{
			var description = _descriptionTest.DescriptionFor(nameof(IDescriptionTest.Name));
			Assert.Equal(TestDescription, description);
			description = _descriptionTest.DescriptionFor("Name");
			Assert.Equal(TestDescription, description);
		}
	}
}