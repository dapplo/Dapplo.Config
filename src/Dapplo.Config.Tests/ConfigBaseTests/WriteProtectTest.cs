// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	/// <summary>
	///     This test class shows how the write protect works
	/// </summary>
	public class WriteProtectTest
	{
		private const string TestValue1 = "VALUE1";
		private readonly IWriteProtectTest _writeProtectTest;

		public WriteProtectTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_writeProtectTest = DictionaryConfiguration<IWriteProtectTest>.Create();
        }

		[Fact]
		public void TestAccessViolation()
		{
			_writeProtectTest.WriteProtect(nameof(IWriteProtectTest.Name));
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Name)));

			Assert.Throws<AccessViolationException>(() => _writeProtectTest.Name = TestValue1);
		}

		[Fact]
		public void TestDisableWriteProtect()
		{
			_writeProtectTest.StartWriteProtecting();
			_writeProtectTest.Age = 30;
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
			_writeProtectTest.StopWriteProtecting();
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
			_writeProtectTest.DisableWriteProtect(nameof(IWriteProtectTest.Age));
			Assert.False(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
		}

		[Fact]
		public void TestRemoveWriteProtect()
		{
			_writeProtectTest.StartWriteProtecting();
			_writeProtectTest.Age = 30;
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));

			_writeProtectTest.RemoveWriteProtection();
			Assert.False(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
		}

		[Fact]
		public void TestWriteProtect()
		{
			_writeProtectTest.StartWriteProtecting();
			_writeProtectTest.Age = 30;
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
			_writeProtectTest.StopWriteProtecting();
			Assert.True(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Age)));
			_writeProtectTest.Name = TestValue1;
			Assert.False(_writeProtectTest.IsWriteProtected(nameof(IWriteProtectTest.Name)));
		}
	}
}