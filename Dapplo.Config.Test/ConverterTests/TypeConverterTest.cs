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

using System;
using Dapplo.Config.Support;
using System.ComponentModel;
using Dapplo.Config.Converters;
using Xunit.Abstractions;
using Dapplo.LogFacade;
using Xunit;

namespace Dapplo.Config.Test.ConverterTests
{
	public class TypeConverterTest
	{
		public TypeConverterTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}

		[Fact]
		public void TestStringEncryptionTypeConverter()
		{
			var stringEncryptionTypeConverter = (TypeConverter) Activator.CreateInstance(typeof (StringEncryptionTypeConverter));
			Assert.True(stringEncryptionTypeConverter.CanConvertFrom(typeof (string)));
			Assert.True(stringEncryptionTypeConverter.CanConvertTo(typeof (string)));
			Assert.False(stringEncryptionTypeConverter.CanConvertTo(typeof (int)));
			var encrypted = stringEncryptionTypeConverter.ConvertToString("Robin");
			var decryped = stringEncryptionTypeConverter.ConvertFromString(encrypted);
			
			Assert.Equal("Robin", decryped);

			var encrypted1 = typeof(string).ConvertOrCastValueToType("Robin", stringEncryptionTypeConverter, convertFrom: false);
			var decryped2 = typeof(string).ConvertOrCastValueToType(encrypted1, stringEncryptionTypeConverter, convertFrom: true);
			Assert.Equal("Robin", decryped2);
		}
	}
}