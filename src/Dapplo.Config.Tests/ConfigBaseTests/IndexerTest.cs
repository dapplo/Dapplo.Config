// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Intercepting;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConfigBaseTests
{
    public class IndexerTest
	{
		private readonly IIndexerTest _indexerTest;
        private readonly ConfigurationBase _indexerImpl;

        public IndexerTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_indexerTest = DictionaryConfiguration<IIndexerTest>.Create();

			_indexerImpl = (_indexerTest as ConfigProxy).Target;
		}

		[Fact]
		public void TestIndexer()
		{
			const string testValue = "Robin";
			_indexerTest.Name = testValue;
			Assert.Equal(testValue, _indexerTest["Name"]);
		}

        [Fact]
        public void TestIndexer_NoItemProperty()
        {
            Assert.Single(_indexerImpl.PropertyNames());
            // We should only have one property
            Assert.Equal(new []{ "Name"}, _indexerImpl.PropertyNames());
        }
    }
}