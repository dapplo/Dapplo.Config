// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

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
			object senderCheck = null;
            var propChanging = new PropertyChangingEventHandler((sender, eventArgs) =>
			{
				senderCheck = sender;
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
			Assert.Equal(_notifyPropertyChangingTest, senderCheck);
        }
	}
}