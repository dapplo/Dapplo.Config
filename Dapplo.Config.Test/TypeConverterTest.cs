using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapplo.Config.Ini;
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

			var stringList = (List<string>) stringListConverter.ConvertFrom(myTestStringWithStrings);
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

			var intList = (List<int>) intListConverter.ConvertFrom(myTestStringWithInts);
			Assert.AreEqual(myTestIntList.Count, intList.Count);
			for (int i = 0; i < myTestIntList.Count; i++)
			{
				Assert.AreEqual(myTestIntList[i], intList[i]);
			}
		}
	}
}