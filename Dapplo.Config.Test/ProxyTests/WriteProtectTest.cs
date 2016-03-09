//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System;
using Dapplo.Config.Test.ProxyTests.Interfaces;
using Dapplo.LogFacade;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Test.ProxyTests
{
	/// <summary>
	///     This test class shows how the write protect works
	/// </summary>
	public class WriteProtectTest
	{
		private const string TestValue1 = "VALUE1";
		private readonly IPropertyProxy<IWriteProtectTest> _propertyProxy;

		public WriteProtectTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
			_propertyProxy = ProxyBuilder.CreateProxy<IWriteProtectTest>();
		}

		[Fact]
		public void TestAccessViolation()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.WriteProtect(x => x.Name);
			Assert.True(properties.IsWriteProtected(x => x.Name));

			var ex = Assert.Throws<AccessViolationException>(() => properties.Name = TestValue1);
		}

		[Fact]
		public void TestDisableWriteProtect()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.StartWriteProtecting();
			properties.Age = 30;
			Assert.True(properties.IsWriteProtected(x => x.Age));
			properties.StopWriteProtecting();
			Assert.True(properties.IsWriteProtected(x => x.Age));
			properties.DisableWriteProtect(x => x.Age);
			Assert.False(properties.IsWriteProtected(x => x.Age));
		}

		[Fact]
		public void TestWriteProtect()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.StartWriteProtecting();
			properties.Age = 30;
			Assert.True(properties.IsWriteProtected(x => x.Age));
			properties.StopWriteProtecting();
			Assert.True(properties.IsWriteProtected(x => x.Age));
			properties.Name = TestValue1;
			Assert.False(properties.IsWriteProtected(x => x.Name));
		}
	}
}