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

using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test
{
	/// <summary>
	/// This test class tests the transactional capabilities of the proxy
	/// </summary>
	[TestClass]
	public class TransactionTest
	{
		private IPropertyProxy<ITransactionTest> _propertyProxy;

		[TestInitialize]
		public void Initialize()
		{
			_propertyProxy = ProxyBuilder.CreateProxy<ITransactionTest>();
		}

		[TestMethod]
		public void TestTransactionCommit()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.AreEqual(30, properties.Age);
			Assert.IsFalse(properties.IsTransactionDirty());
			properties.Age = 35;
			Assert.IsTrue(properties.IsTransactionDirty());
			Assert.AreEqual(35, properties.Age);
			properties.CommitTransaction();
			Assert.AreEqual(35, properties.Age);
		}

		[TestMethod]
		public void TestTransactionRollback()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.AreEqual(properties.Age, 30);
			properties.Age = 35;
			Assert.AreEqual(properties.Age, 35);
			properties.RollbackTransaction();
			Assert.AreEqual(properties.Age, 30);
		}
	}
}