/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
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

using System;
using Dapplo.Config.Test.ProxyTests.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test.ProxyTests
{
	/// <summary>
	/// This test class shows how the write protect works
	/// </summary>
	[TestClass]
	public class WriteProtectTest
	{
		private const string TestValue1 = "VALUE1";
		private IPropertyProxy<IWriteProtectTest> _propertyProxy;

		[TestInitialize]
		public void Initialize()
		{
			_propertyProxy = ProxyBuilder.CreateProxy<IWriteProtectTest>();
		}

		[TestMethod]
		public void TestWriteProtect()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.StartWriteProtecting();
			properties.Age = 30;
			Assert.IsTrue(properties.IsWriteProtected(x => x.Age));
			properties.StopWriteProtecting();
			Assert.IsTrue(properties.IsWriteProtected(x => x.Age));
			properties.Name = TestValue1;
			Assert.IsFalse(properties.IsWriteProtected(x => x.Name));
		}

		[TestMethod]
		public void TestDisableWriteProtect()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.StartWriteProtecting();
			properties.Age = 30;
			Assert.IsTrue(properties.IsWriteProtected(x => x.Age));
			properties.StopWriteProtecting();
			Assert.IsTrue(properties.IsWriteProtected(x => x.Age));
			properties.DisableWriteProtect(x => x.Age);
			Assert.IsFalse(properties.IsWriteProtected(x => x.Age));
		}

		[TestMethod]
		[ExpectedException(typeof (AccessViolationException))]
		public void TestAccessViolation()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.WriteProtect(x => x.Name);
			Assert.IsTrue(properties.IsWriteProtected(x => x.Name));
			// This should throw a AccessViolationException
			properties.Name = TestValue1;
		}
	}
}