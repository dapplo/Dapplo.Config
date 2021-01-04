// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Dapplo.Utils.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.ConverterTests
{
	public class TypeExtensionsTests
	{
		public TypeExtensionsTests(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
		}

		[Fact]
		public void TestConvertOrCastValueToType_DictionaryWithUris()
		{
			var testUri = new Uri("http://test.com/dapplo?name=config");

			var testValues = new Dictionary<string, Uri> {{"value1", testUri}};

			var stringDictionary = typeof(IDictionary<string, string>).ConvertOrCastValueToType(testValues, convertFrom: false) as IDictionary<string, string>;
			Assert.NotNull(stringDictionary);
			Assert.Equal(testValues["value1"].AbsoluteUri, stringDictionary["value1"]);

			var uriDictionary = typeof(Dictionary<string, Uri>).ConvertOrCastValueToType(stringDictionary) as IDictionary<string, Uri>;
			Assert.NotNull(uriDictionary);
			Assert.Equal(testValues["value1"].AbsoluteUri, uriDictionary["value1"].AbsoluteUri);
		}

		[Fact]
		public void TestConvertOrCastValueToType_Enum()
		{
			var val1 = typeof(TestEnum).ConvertOrCastValueToType("VAL_NOT");
			Assert.NotNull(val1);
		}

		[Fact]
		public void TestConvertOrCastValueToType_Uri()
		{
			var testUri = new Uri("http://test.com/dapplo?name=config");

			var stringUri = typeof(string).ConvertOrCastValueToType(testUri, convertFrom: false);
			Assert.Equal(testUri.AbsoluteUri, stringUri);

			var convertedUri = typeof(Uri).ConvertOrCastValueToType(stringUri) as Uri;
			Assert.Equal(testUri, convertedUri);
		}
	}
}