//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
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

using System;
using Dapplo.Config.Tests.ConfigBaseTests.Entities;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

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
			_writeProtectTest = new WriteProtectImpl();
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