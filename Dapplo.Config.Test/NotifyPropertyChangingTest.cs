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
	public class NotifyPropertyChangingTest {
		private IPropertyProxy<INotifyPropertyChangingTest> _propertyProxy;

		private const string NoChange = "NOCHANGE";
		private const string TestValue1 = "VALUE1";
		private const string TestValue2 = "VALUE2";

		[TestInitialize]
		public void Initialize() {
			_propertyProxy = ProxyBuilder.CreateProxy<INotifyPropertyChangingTest>();
		}

		[TestMethod]
		public void TestNotifyPropertyChanging() {

			var properties = _propertyProxy.PropertyObject;
			string changingPropertyName = null;

			var propChanging = new PropertyChangingEventHandler((sender, eventArgs) => {
				changingPropertyName = eventArgs.PropertyName;
			});

			// Test event handler
			properties.PropertyChanging += propChanging;
			properties.Name = TestValue1;
			Assert.AreEqual("Name", changingPropertyName);

			// Ensure that if the value is the same, we don't get an event
			changingPropertyName = NoChange;
			properties.Name = TestValue1;
			Assert.AreEqual(NoChange, changingPropertyName);

			// Test if event handler is unregistered
			properties.PropertyChanging -= propChanging;
			changingPropertyName = NoChange;
			properties.Name = TestValue2;
			Assert.AreEqual(NoChange, changingPropertyName);
		}
	}
}