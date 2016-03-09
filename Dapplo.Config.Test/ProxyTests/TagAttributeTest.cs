//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using Dapplo.Config.Test.ProxyTests.Interfaces;
using Dapplo.LogFacade;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Test.ProxyTests
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
		private readonly IPropertyProxy<ITagAttributeTest> _propertyProxy;

		public TagAttributeTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
			_propertyProxy = ProxyBuilder.CreateProxy<ITagAttributeTest>();
		}

		[Fact]
		public void TestTagging()
		{
			var properties = _propertyProxy.PropertyObject;
			Assert.False(properties.IsTaggedWith(x => x.Name, "Expert"));
			Assert.False(properties.IsTaggedWith("Name", "Expert"));

			Assert.True(properties.IsTaggedWith(x => x.Age, "Expert"));
			Assert.True(properties.IsTaggedWith("Age", "Expert"));

			Assert.False(properties.IsTaggedWith(x => x.Age, "Expert2"));
			Assert.False(properties.IsTaggedWith("Age", "Expert2"));

			Assert.True(properties.IsTaggedWith(x => x.FirstName, TestTags.Tag2));
			Assert.True(properties.IsTaggedWith("FirstName", TestTags.Tag2));

			Assert.True(properties.IsTaggedWith(x => x.FirstName, TestTags.Tag1));
			Assert.True(properties.IsTaggedWith("FirstName", TestTags.Tag1));
			// Test if we can access the value of a tag
			Assert.Equal("Robin", properties.GetTagValue("FirstName", TestTags.Tag1));

			Assert.False(properties.IsTaggedWith(x => x.FirstName, TestTags.Expert));
			Assert.False(properties.IsTaggedWith("FirstName", TestTags.Expert));
		}
	}
}