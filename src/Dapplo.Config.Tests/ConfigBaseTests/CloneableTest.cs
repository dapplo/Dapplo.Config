// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	/// <summary>
	/// Tests for the IShallowCloneable extension
	/// </summary>
	public class CloneableTest
	{
		private readonly ICloneableTest _cloneableTest;

		public CloneableTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_cloneableTest = DictionaryConfiguration<ICloneableTest>.Create();
        }

		[Fact]
		public void TestClone()
		{
			const string testValue = "Robin";
			_cloneableTest.Name = testValue;
			var cloned = (ICloneableTest) _cloneableTest.ShallowClone();
			Assert.Equal(testValue, cloned.Name);
			cloned.Name = "Dapplo";
			// The old instance should still have the previous value
			Assert.Equal(testValue, _cloneableTest.Name);
			// The cloned instance should NOT have the previous value
			Assert.NotEqual(testValue, cloned.Name);
		}
	}
}