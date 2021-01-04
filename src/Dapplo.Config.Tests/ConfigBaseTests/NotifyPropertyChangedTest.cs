// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Dapplo.Utils.Notify;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	public class NotifyPropertyChangedTest
	{
		private static readonly LogSource Log = new LogSource();
		private const string NoChange = "NOCHANGE";
		private const string TestValue1 = "VALUE1";
		private const string TestValue2 = "VALUE2";
		private readonly INotifyPropertyChangedTest _notifyPropertyChangedTest;

		public NotifyPropertyChangedTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_notifyPropertyChangedTest = DictionaryConfiguration<INotifyPropertyChangedTest>.Create();
		}

		/// <summary>
		/// Test with Dapplo.Utils EventObservable.From
		/// </summary>
		[Fact]
		public void TestNotifyPropertyChanged_EventObservable()
		{
			PropertyChangedEventArgs testValue = null;
			using (_notifyPropertyChangedTest.OnPropertyChanged().Subscribe(pce => testValue = pce))
			{
				_notifyPropertyChangedTest.Name = "Test";
				Assert.Equal("Name", testValue.PropertyName);
			}
		}

		[Fact]
		public void TestNotifyPropertyChanged()
		{
			string changedPropertyName = null;
			object senderCheck = null;
			var propChanged = new PropertyChangedEventHandler((sender, eventArgs) =>
			{
				senderCheck = sender;
                changedPropertyName = eventArgs.PropertyName;
				Log.Debug().WriteLine("Property change notification for {0}", eventArgs.PropertyName);
			});
            _notifyPropertyChangedTest.PropertyChanged += propChanged;

            // Test event handler
            _notifyPropertyChangedTest.Name = TestValue1;
			Assert.Equal("Name", changedPropertyName);

			// Ensure that if the value is the same, we don't get an event
			changedPropertyName = NoChange;
			_notifyPropertyChangedTest.Name = TestValue1;
			Assert.Equal(NoChange, changedPropertyName);

			// Test if event handler is unregistered
			_notifyPropertyChangedTest.PropertyChanged -= propChanged;
			changedPropertyName = NoChange;
			_notifyPropertyChangedTest.Name = TestValue2;
			Assert.Equal(NoChange, changedPropertyName);
			Assert.Equal(_notifyPropertyChangedTest, senderCheck);
		}
	}
}