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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test {
	/// <summary>
	/// This test class shows how the write protect works
	/// </summary>
	[TestClass]
	public class WriteProtectTest {
		private const string TestValue1 = "VALUE1";
		private IPropertyProxy<IPersonProperties> _propertyProxy;
	
		[TestInitialize]
		public void Initialize() {
			_propertyProxy = ProxyBuilder.CreateProxy<IPersonProperties>();
		}

		[TestMethod]
		public void TestWriteProtect() {

			var properties = _propertyProxy.PropertyObject;
			properties.StartWriteProtecting();
			properties.Age = 30;
			Assert.IsTrue(properties.IsWriteProtected(x => x.Age));
			properties.StopWriteProtecting();
			properties.Name = TestValue1;
			Assert.IsFalse(properties.IsWriteProtected(x => x.Name));
			properties.WriteProtect(x => x.Name);
			Assert.IsTrue(properties.IsWriteProtected(x => x.Name));
			try {
				properties.Name = TestValue1;
				Assert.Fail("Exception expected!");
			} catch(Exception ex) {
				Assert.AreEqual(ex.GetType(), typeof(AccessViolationException));
			}
		}
	}
}