/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Dapplo.Config.Support;

namespace Dapplo.Config.Test.ConfigTests
{
	[TestClass]
	public class ConvertTest
	{
		[TestMethod]
		public void TestConvertCollection()
		{
			var collection = TypeExtensions.ConvertOrCastValueToType<ICollection<string>>("1,2,3, \"4 , 5\"  ,5,6");
			Assert.IsTrue(collection.Count == 6);
			var list = TypeExtensions.ConvertOrCastValueToType<IList<string>>("1,2,3, 4  ,5,6");
			Assert.IsTrue(list.Count == 6);
			var intList = TypeExtensions.ConvertOrCastValueToType<List<int>>("1,2,3, 4  ,5,6");
			Assert.IsTrue(intList.Count == 6);
			var set = TypeExtensions.ConvertOrCastValueToType<ISet<string>>("1,2,3, 4  ,5,6");
			Assert.IsTrue(set.Count == 6);
		}

		[TestMethod]
		public void TestConvertDictionary()
		{
			var instance = TypeExtensions.ConvertOrCastValueToType<IDictionary<string, string>>("name1=value1,\"name2=value2 and something more, like a comma\",name3=value3,name4=value4");

			Assert.IsTrue(instance.Count == 4);

			var instance2 = TypeExtensions.ConvertOrCastValueToType<IDictionary<int, double>>("10=100,20=200,30=300,40=400");

			Assert.IsTrue(instance2.Count == 4);

			var sourceDictionary = new Dictionary<string, int>
			{
				{"value1", 10 },
				{"value2", 20 }
			};
			var instance3 = TypeExtensions.ConvertOrCastValueToType<IDictionary<string, double>>(sourceDictionary);
			Assert.IsTrue(instance3.Count == 2);
			TypeExtensions.ConvertOrCastValueToType<IDictionary<string, string>>(instance3);
        }
	}
}
