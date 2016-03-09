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

using Dapplo.Config.Test.ProxyTests.Interfaces;
using Dapplo.LogFacade;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Test.ProxyTests
{
	/// <summary>
	///     Test case to show how the default value works
	/// </summary>
	public class DefaultValueTest
	{
		private readonly IPropertyProxy<IDefaultValueTest> _propertyProxy;

		public DefaultValueTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
			_propertyProxy = ProxyBuilder.CreateProxy<IDefaultValueTest>();
		}

		[Fact]
		public void TestDefaultValue()
		{
			var properties = _propertyProxy.PropertyObject;
			Assert.Equal(properties.Age, 21);
			Assert.Equal(3, properties.Ages.Count);
		}

		[Fact]
		public void TestDefaultValueAtrribute()
		{
			var properties = _propertyProxy.PropertyObject;
			var defaultValue = properties.DefaultValueFor(x => x.Age);
			Assert.Equal(defaultValue, 21);
			defaultValue = properties.DefaultValueFor("Age");
			Assert.Equal(defaultValue, 21);
		}

		[Fact]
		public void TestDefaultValueWithError()
		{
			// Used to be:
			//var ex = Assert.Throws<InvalidCastException>(() => ProxyBuilder.CreateProxy<IDefaultValueWithErrorTest>());
			// Now it should run without error
			ProxyBuilder.CreateProxy<IDefaultValueWithErrorTest>();
		}
	}
}