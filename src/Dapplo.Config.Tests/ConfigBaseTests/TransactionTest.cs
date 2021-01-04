// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	/// <summary>
	///     This test class tests the transactional capabilities of the proxy
	/// </summary>
	public class TransactionTest
	{
		private readonly ITransactionTest _transactionTest;

		public TransactionTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);

			_transactionTest = DictionaryConfiguration<ITransactionTest>.Create();
        }

		[Fact]
		public void TestTransactionCommit()
		{
			var properties = _transactionTest;
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
			var properties = _transactionTest;
			properties.Age = 30;
			properties.StartTransaction();
			Assert.Equal(30, properties.Age);
			properties.Age = 35;
			Assert.Equal(35, properties.Age);
			properties.RollbackTransaction();
			Assert.Equal(30, properties.Age);
		}
	}
}