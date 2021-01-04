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
	///     Test case to show how the HasChanges works
	/// </summary>
	public class HasChangesTests
	{
		private readonly IHasChangesTest _hasChangesTest;

		public HasChangesTests(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_hasChangesTest = DictionaryConfiguration<IHasChangesTest>.Create();
			_hasChangesTest.TrackChanges();
		}

		[Fact]
		public void TestHasChanges()
		{
			_hasChangesTest.SayMyName = "Robin";
			Assert.True(_hasChangesTest.HasChanges());
			Assert.True(_hasChangesTest.IsChanged(nameof(_hasChangesTest.SayMyName)));
			Assert.True(_hasChangesTest.IsChanged(nameof(IHasChangesTest.SayMyName)));
			Assert.True(_hasChangesTest.Changes().Contains(nameof(_hasChangesTest.SayMyName)));
			_hasChangesTest.ResetHasChanges();
			Assert.False(_hasChangesTest.HasChanges());
			Assert.False(_hasChangesTest.IsChanged(nameof(_hasChangesTest.SayMyName)));
			Assert.False(_hasChangesTest.IsChanged(nameof(IHasChangesTest.SayMyName)));
			Assert.False(_hasChangesTest.Changes().Contains(nameof(_hasChangesTest.SayMyName)));
		}
	}
}