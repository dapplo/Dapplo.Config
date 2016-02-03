/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Test.ProxyTests.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test.ProxyTests
{
	public enum TestTags
	{
		Tag1,
		Expert,
		Tag2
	}

	/// <summary>
	/// This test class shows how the expert attribute can be used
	/// </summary>
	[TestClass]
	public class TagAttributeTest
	{
		private IPropertyProxy<ITagAttributeTest> _propertyProxy;

		[TestInitialize]
		public void Initialize()
		{
			_propertyProxy = ProxyBuilder.CreateProxy<ITagAttributeTest>();
		}

		[TestMethod]
		public void TestTagging()
		{
			var properties = _propertyProxy.PropertyObject;
			Assert.IsFalse(properties.IsTaggedWith(x => x.Name, "Expert"));
			Assert.IsFalse(properties.IsTaggedWith("Name", "Expert"));

			Assert.IsTrue(properties.IsTaggedWith(x => x.Age, "Expert"));
			Assert.IsTrue(properties.IsTaggedWith("Age", "Expert"));

			Assert.IsFalse(properties.IsTaggedWith(x => x.Age, "Expert2"));
			Assert.IsFalse(properties.IsTaggedWith("Age", "Expert2"));

			Assert.IsTrue(properties.IsTaggedWith(x => x.FirstName, TestTags.Tag2));
			Assert.IsTrue(properties.IsTaggedWith("FirstName", TestTags.Tag2));

			Assert.IsTrue(properties.IsTaggedWith(x => x.FirstName, TestTags.Tag1));
			Assert.IsTrue(properties.IsTaggedWith("FirstName", TestTags.Tag1));
			// Test if we can access the value of a tag
			Assert.AreEqual("Robin", properties.GetTagValue("FirstName", TestTags.Tag1));

			Assert.IsFalse(properties.IsTaggedWith(x => x.FirstName, TestTags.Expert));
			Assert.IsFalse(properties.IsTaggedWith("FirstName", TestTags.Expert));
		}
	}
}