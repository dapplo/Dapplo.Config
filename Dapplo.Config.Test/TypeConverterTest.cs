/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
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

using Dapplo.Config.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dapplo.Config.Test
{
	[TestClass]
	public class TypeConverterTest
	{
		[TestMethod]
		public void TestGenericListConverterString()
		{
			var stringListConverter = (TypeConverter) Activator.CreateInstance(typeof (StringToGenericListConverter<string>));
			Assert.IsTrue(stringListConverter.CanConvertFrom(typeof (string)));
			Assert.IsTrue(stringListConverter.CanConvertTo(typeof (string)));
			Assert.IsFalse(stringListConverter.CanConvertTo(typeof (List<string>)));

			var intListConverter = (TypeConverter) Activator.CreateInstance(typeof (StringToGenericListConverter<int>));
			Assert.IsTrue(intListConverter.CanConvertFrom(typeof (string)));
			Assert.IsTrue(intListConverter.CanConvertTo(typeof (string)));

			IList<string> myTestStringList = new List<string>();
			myTestStringList.Add("hello");
			myTestStringList.Add("goodbye");
			myTestStringList.Add("adieu");

			var myTestStringWithStrings = string.Join(",", myTestStringList);

			var stringList = (List<string>) stringListConverter.ConvertFromInvariantString(myTestStringWithStrings);
			Assert.AreEqual(myTestStringList.Count, stringList.Count);
			for (int i = 0; i < myTestStringList.Count; i++)
			{
				Assert.AreEqual(myTestStringList[i], stringList[i]);
			}

			IList<int> myTestIntList = new List<int>();
			myTestIntList.Add(10);
			myTestIntList.Add(20);
			myTestIntList.Add(30);

			var myTestStringWithInts = string.Join(",", myTestIntList);

			var intList = (List<int>) intListConverter.ConvertFromInvariantString(myTestStringWithInts);
			Assert.AreEqual(myTestIntList.Count, intList.Count);
			for (int i = 0; i < myTestIntList.Count; i++)
			{
				Assert.AreEqual(myTestIntList[i], intList[i]);
			}
		}
	}
}