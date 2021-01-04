// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	public class BassicAssignTest
	{
		private readonly IBassicAssignTest _bassicAssignTest;

		public BassicAssignTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_bassicAssignTest = DictionaryConfiguration<IBassicAssignTest>.Create();
        }

		[Fact]
		public void TestAssign()
		{
			const string testValue = "Robin";
			_bassicAssignTest.Name = testValue;
			Assert.Equal(testValue, _bassicAssignTest.Name);
		}
	}
}