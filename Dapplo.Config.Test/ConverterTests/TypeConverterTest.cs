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

using System;
using Dapplo.Config.Support;
using System.ComponentModel;
using Dapplo.Config.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test.ConverterTests
{
	[TestClass]
	public class TypeConverterTest
	{
		[TestMethod]
		public void TestStringEncryptionTypeConverter()
		{
			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
			var stringEncryptionTypeConverter = (TypeConverter) Activator.CreateInstance(typeof (StringEncryptionTypeConverter));
			Assert.IsTrue(stringEncryptionTypeConverter.CanConvertFrom(typeof (string)));
			Assert.IsTrue(stringEncryptionTypeConverter.CanConvertTo(typeof (string)));
			Assert.IsFalse(stringEncryptionTypeConverter.CanConvertTo(typeof (int)));
			var encrypted = stringEncryptionTypeConverter.ConvertToString("Robin");
			var decryped = stringEncryptionTypeConverter.ConvertFromString(encrypted);
			
			Assert.AreEqual("Robin", decryped);

			var encrypted1 = typeof(string).ConvertOrCastValueToType("Robin", stringEncryptionTypeConverter, convertFrom: false);
			var decryped2 = typeof(string).ConvertOrCastValueToType(encrypted1, stringEncryptionTypeConverter, convertFrom: true);
			Assert.AreEqual("Robin", decryped2);
		}
	}
}