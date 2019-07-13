//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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

using System.ComponentModel;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	public class NotifyPropertyChangingTest
	{
		private static readonly LogSource Log = new LogSource();
		private const string NoChange = "NOCHANGE";
		private const string TestValue1 = "VALUE1";
		private const string TestValue2 = "VALUE2";
		private readonly INotifyPropertyChangingTest _notifyPropertyChangingTest;

		public NotifyPropertyChangingTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_notifyPropertyChangingTest = DictionaryConfiguration<INotifyPropertyChangingTest>.Create();
		}

		[Fact]
		public void TestNotifyPropertyChanging()
		{
			string changingPropertyName = null;

			var propChanging = new PropertyChangingEventHandler((sender, eventArgs) =>
			{
				changingPropertyName = eventArgs.PropertyName;
				Log.Debug().WriteLine("Property change notification for {0}", eventArgs.PropertyName);
			});

			// Test event handler
			_notifyPropertyChangingTest.PropertyChanging += propChanging;
			_notifyPropertyChangingTest.Name = TestValue1;

			// Event can't be called on a task, otherwise this would fail
			Assert.Equal("Name", changingPropertyName);

			// Ensure that if the value is the same, we don't get an event
			changingPropertyName = NoChange;
			_notifyPropertyChangingTest.Name = TestValue1;
			Assert.Equal(NoChange, changingPropertyName);

			// Test if event handler is unregistered
			_notifyPropertyChangingTest.PropertyChanging -= propChanging;
			changingPropertyName = NoChange;
			_notifyPropertyChangingTest.Name = TestValue2;
			Assert.Equal(NoChange, changingPropertyName);
		}
	}
}