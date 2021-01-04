// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	public enum TestTags
	{
		Tag1,
		Expert,
		Tag2
	}

	/// <summary>
	///     This test class shows how the expert attribute can be used
	/// </summary>
	public class TagAttributeTest
	{
		private readonly ITagAttributeTest _tagAttributeTest;

		public TagAttributeTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_tagAttributeTest = DictionaryConfiguration<ITagAttributeTest>.Create();
		}

		[Fact]
		public void TestTagging()
		{
			Assert.False(_tagAttributeTest.IsTaggedWith(nameof(ITagAttributeTest.Name), "Expert"));

			Assert.True(_tagAttributeTest.IsTaggedWith(nameof(ITagAttributeTest.Age), "Expert"));

			Assert.False(_tagAttributeTest.IsTaggedWith(nameof(ITagAttributeTest.Age), "Expert2"));

			Assert.True(_tagAttributeTest.IsTaggedWith(nameof(ITagAttributeTest.FirstName), TestTags.Tag2));

			Assert.True(_tagAttributeTest.IsTaggedWith(nameof(ITagAttributeTest.FirstName), TestTags.Tag1));
			// Test if we can access the value of a tag
			Assert.Equal("Robin", _tagAttributeTest.GetTagValue(nameof(ITagAttributeTest.FirstName), TestTags.Tag1));

			Assert.False(_tagAttributeTest.IsTaggedWith(nameof(ITagAttributeTest.FirstName), TestTags.Expert));
		}
	}
}