// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel;
using Dapplo.Config.Ini.Converters;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Dapplo.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConverterTests
{
	public class TypeConverterTest
	{
		public TypeConverterTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911f";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}

		[Fact]
		public void TestStringEncryptionTypeConverter()
		{
			var stringEncryptionTypeConverter = (TypeConverter) Activator.CreateInstance(typeof(StringEncryptionTypeConverter));
			Assert.True(stringEncryptionTypeConverter.CanConvertFrom(typeof(string)));
			Assert.True(stringEncryptionTypeConverter.CanConvertTo(typeof(string)));
			Assert.False(stringEncryptionTypeConverter.CanConvertTo(typeof(int)));
			var encrypted = stringEncryptionTypeConverter.ConvertToString("Robin");
			var decrypted = stringEncryptionTypeConverter.ConvertFromString(encrypted);

			Assert.Equal("Robin", decrypted);

			var encrypted1 = typeof(string).ConvertOrCastValueToType("Robin", stringEncryptionTypeConverter, convertFrom: false);
			var decrypted2 = typeof(string).ConvertOrCastValueToType(encrypted1, stringEncryptionTypeConverter);
			Assert.Equal("Robin", decrypted2);
		}
	}
}