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
using Dapplo.Config.Test.TestInterfaces;

namespace Dapplo.Config.Test {
	[TestClass]
	public class NotifyPropertyChangedTest {
		private IPropertyProxy<INotifyPropertyChangedTest> _propertyProxy;

		private const string NoChange = "NOCHANGE";
		private const string TestValue1 = "VALUE1";
		private const string TestValue2 = "VALUE2";

		[TestInitialize]
		public void Initialize() {
			_propertyProxy = ProxyBuilder.CreateProxy<INotifyPropertyChangedTest>();
		}

		[TestMethod]
		public void TestNotifyPropertyChanged() {

			var properties = _propertyProxy.PropertyObject;
			string changedPropertyName = null;

			var propChanged = new PropertyChangedEventHandler((sender, eventArgs) => {
				changedPropertyName = eventArgs.PropertyName;
			});

			// Test event handler
			properties.PropertyChanged += propChanged;
			properties.Name = TestValue1;
			Assert.AreEqual("Name", changedPropertyName);

			// Ensure that if the value is the same, we don't get an event
			changedPropertyName = NoChange;
			properties.Name = TestValue1;
			Assert.AreEqual(NoChange, changedPropertyName);

			// Test if event handler is unregistered
			properties.PropertyChanged -= propChanged;
			changedPropertyName = NoChange;
			properties.Name = TestValue2;
			Assert.AreEqual(NoChange, changedPropertyName);
		}
	}
}