/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Config

	Dapplo.Config is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Config is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have Config a copy of the GNU Lesser General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.
 */

using Dapplo.Config.Test.ProxyTests.Interfaces;
using Dapplo.LogFacade;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Test.ProxyTests
{
	/// <summary>
	/// This test class tests the transactional capabilities of the proxy
	/// </summary>
	public class TransactionTest
	{
		private IPropertyProxy<ITransactionTest> _propertyProxy;

		public TransactionTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
			_propertyProxy = ProxyBuilder.CreateProxy<ITransactionTest>();
		}

		[Fact]
		public void TestTransactionCommit()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.Equal(30, properties.Age);
			Assert.False(properties.IsTransactionDirty());
			properties.Age = 35;
			Assert.True(properties.IsTransactionDirty());
			Assert.Equal(35, properties.Age);
			properties.CommitTransaction();
			Assert.Equal(35, properties.Age);
		}

		[Fact]
		public void TestTransactionRollback()
		{
			var properties = _propertyProxy.PropertyObject;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.Equal(properties.Age, 30);
			properties.Age = 35;
			Assert.Equal(properties.Age, 35);
			properties.RollbackTransaction();
			Assert.Equal(properties.Age, 30);
		}
	}
}