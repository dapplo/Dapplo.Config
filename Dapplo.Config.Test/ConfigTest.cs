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

using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dapplo.Config.Test {
	[TestClass]
	public class ConfigTest {
		private IPropertyProxy<IPersonProperties> _propertyProxy;

		[TestInitialize]
		public void Initialize() {
			_propertyProxy = ProxyBuilder.CreateProxy<IPersonProperties>();
		}

		[TestMethod]
		public void TestDefaultValue() {
			var properties = _propertyProxy.PropertyObject;
			Assert.AreEqual(properties.Age, 21);
		}

		[TestMethod]
		public void TestExpert() {
			var properties = _propertyProxy.PropertyObject;
			Assert.IsFalse(properties.IsExpert(x => x.Name));
			Assert.IsTrue(properties.IsExpert(x => x.Age));
		}

		[TestMethod]
		public void TestNotifyPropertyChanged() {
			const string NOCHANGE = "NOCHANGE";
			const string TEST_VALUE_1 = "VALUE1";
			const string TEST_VALUE_2 = "VALUE2";
			var properties = _propertyProxy.PropertyObject;
			string changedPropertyName = null;

			var propChanged = new PropertyChangedEventHandler((sender, eventArgs) => {
				changedPropertyName = eventArgs.PropertyName;
			});

			// Test event handler
			properties.PropertyChanged += propChanged;
			properties.Name = TEST_VALUE_1;
			Assert.AreEqual("Name", changedPropertyName);

			// Test if the value doesn't change
			changedPropertyName = NOCHANGE;
			properties.Name = TEST_VALUE_1;
			Assert.AreEqual(NOCHANGE, changedPropertyName);

			// Test if event handler is unregistered
			properties.PropertyChanged -= propChanged;
			changedPropertyName = NOCHANGE;
			properties.Name = TEST_VALUE_2;
			Assert.AreEqual(NOCHANGE, changedPropertyName);
		}

		[TestMethod]
		public void TestAssign() {
			var properties = _propertyProxy.PropertyObject;
			const string testValue = "Robin";
			properties.Name = testValue;
			Assert.AreEqual(testValue, properties.Name);
		}

		[TestMethod]
		public void TestTransactionCommit() {
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
		public void TestTransactionRollback() {
			var properties = _propertyProxy.PropertyObject;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.AreEqual(properties.Age, 30);
			properties.Age = 35;
			Assert.AreEqual(properties.Age, 35);
			properties.RollbackTransaction();
			Assert.AreEqual(properties.Age, 30);
		}

		[TestMethod]
		public void TestWriteProtect() {
			const string TEST_VALUE_1 = "VALUE1";

			var properties = _propertyProxy.PropertyObject;
			properties.StartWriteProtecting();
			properties.Age = 30;
			Assert.IsTrue(properties.IsWriteProtected(x => x.Age));
			properties.StopWriteProtecting();
			properties.Name = TEST_VALUE_1;
			Assert.IsFalse(properties.IsWriteProtected(x => x.Name));
			properties.WriteProtect(x => x.Name);
			Assert.IsTrue(properties.IsWriteProtected(x => x.Name));
			try {
				properties.Name = TEST_VALUE_1;
				Assert.Fail("Exception expected!");
			} catch(Exception ex) {
				Assert.AreEqual(ex.GetType(), typeof(AccessViolationException));
			}
		}
	}
}
