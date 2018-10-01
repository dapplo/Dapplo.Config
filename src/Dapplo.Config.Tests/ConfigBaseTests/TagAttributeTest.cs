//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Config
// 
//  Dapplo.Config is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Config is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using Dapplo.Config.Tests.ConfigBaseTests.Entities;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

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
			_tagAttributeTest = new TagAttributeImpl();
		}

		[Fact]
		public void TestTagging()
		{
			Assert.False(_tagAttributeTest.IsTaggedWith(x => x.Name, "Expert"));
			Assert.False(_tagAttributeTest.IsTaggedWith("Name", "Expert"));

			Assert.True(_tagAttributeTest.IsTaggedWith(x => x.Age, "Expert"));
			Assert.True(_tagAttributeTest.IsTaggedWith("Age", "Expert"));

			Assert.False(_tagAttributeTest.IsTaggedWith(x => x.Age, "Expert2"));
			Assert.False(_tagAttributeTest.IsTaggedWith("Age", "Expert2"));

			Assert.True(_tagAttributeTest.IsTaggedWith(x => x.FirstName, TestTags.Tag2));
			Assert.True(_tagAttributeTest.IsTaggedWith("FirstName", TestTags.Tag2));

			Assert.True(_tagAttributeTest.IsTaggedWith(x => x.FirstName, TestTags.Tag1));
			Assert.True(_tagAttributeTest.IsTaggedWith("FirstName", TestTags.Tag1));
			// Test if we can access the value of a tag
			Assert.Equal("Robin", _tagAttributeTest.GetTagValue("FirstName", TestTags.Tag1));

			Assert.False(_tagAttributeTest.IsTaggedWith(x => x.FirstName, TestTags.Expert));
			Assert.False(_tagAttributeTest.IsTaggedWith("FirstName", TestTags.Expert));
		}
	}
}