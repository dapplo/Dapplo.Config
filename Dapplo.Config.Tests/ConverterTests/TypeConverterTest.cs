//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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

using System;
using System.ComponentModel;
using Dapplo.Config.Converters;
using Dapplo.Log.Facade;
using Dapplo.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;
using Dapplo.Log.XUnit;

#endregion

namespace Dapplo.Config.Tests.ConverterTests
{
	public class TypeConverterTest
	{
		public TypeConverterTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
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

			var encrypted1 = typeof (string).ConvertOrCastValueToType("Robin", stringEncryptionTypeConverter, convertFrom: false);
			var decryped2 = typeof (string).ConvertOrCastValueToType(encrypted1, stringEncryptionTypeConverter);
			Assert.Equal("Robin", decryped2);
		}
	}
}