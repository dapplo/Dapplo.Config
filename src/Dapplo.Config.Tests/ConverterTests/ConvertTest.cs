// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Dapplo.Config.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConverterTests
{
	public class ConvertTest
	{
		public ConvertTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
		}

		[Fact]
		public void TestConvertCollection()
		{
			var collection = TypeExtensions.ConvertOrCastValueToType<ICollection<string>>("1,2,3, \"4 , 5\"  ,5,6");
			Assert.Equal(6,collection.Count);
			var list = TypeExtensions.ConvertOrCastValueToType<IList<string>>("1,2,3, 4  ,5,6");
			Assert.Equal(6, list.Count);
			var intList = TypeExtensions.ConvertOrCastValueToType<List<int>>("1,2,3, 4  ,5,6");
			Assert.Equal(6, intList.Count);
			var set = TypeExtensions.ConvertOrCastValueToType<ISet<string>>("1,2,3, 4  ,5,6");
			Assert.Equal(6, set.Count);
		}

		[Fact]
		public void TestConvertDictionary()
		{
			var instance = TypeExtensions.ConvertOrCastValueToType<IDictionary<string, string>>("name1=value1,\"name2=value2 and something more, like a comma\",name3=value3,name4=value4");
			Assert.Equal(4, instance.Count);

			var instance2 = TypeExtensions.ConvertOrCastValueToType<IDictionary<int, double>>("10=100,20=200,30=300,40=400");
			Assert.Equal(4, instance2.Count);

			var sourceDictionary = new Dictionary<string, int>
			{
				{"value1", 10},
				{"value2", 20}
			};
			var instance3 = TypeExtensions.ConvertOrCastValueToType<IDictionary<string, double>>(sourceDictionary);
			Assert.Equal(2, instance3.Count);
			TypeExtensions.ConvertOrCastValueToType<IDictionary<string, string>>(instance3);
		}
	}
}