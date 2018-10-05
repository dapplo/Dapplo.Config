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

using Dapplo.Config.Tests.ConfigBaseTests.Entities;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigBaseTests
{
    public class IndexerTest
	{
		private readonly IIndexerTest _indexerTest;
        private readonly IndexerImpl _indexerImpl;

        public IndexerTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_indexerTest = _indexerImpl = new IndexerImpl();
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
            Assert.Single(_indexerImpl.PropertyNames);
            // We should only have one property
            Assert.Equal(new []{ "Name"}, _indexerImpl.PropertyNames);
        }
    }
}