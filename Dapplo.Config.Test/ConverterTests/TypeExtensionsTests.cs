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

using Dapplo.Config.Support;
using Dapplo.LogFacade;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Test.ConverterTests
{
	public class TypeExtensionsTests
	{
		public TypeExtensionsTests(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
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

		[Fact]
		public void TestConvertOrCastValueToType_DictionaryWithUris()
		{
			var testUri = new Uri("http://test.com/dapplo?name=config");

			var testValues = new Dictionary<string, Uri>();
			testValues.Add("value1", testUri);

			var stringDictionary = typeof(IDictionary<string, string>).ConvertOrCastValueToType(testValues, convertFrom: false) as IDictionary<string, string>;
			Assert.Equal(testValues["value1"].AbsoluteUri, stringDictionary["value1"]);

			var uriDictionary = typeof(Dictionary<string, Uri>).ConvertOrCastValueToType(stringDictionary) as IDictionary<string, Uri>;
			Assert.Equal(testValues["value1"].AbsoluteUri, uriDictionary["value1"].AbsoluteUri);
		}

		[Fact]
		public void TestConvertOrCastValueToType_Enum()
		{
			var val1 = typeof(TestEnum).ConvertOrCastValueToType("VAL_NOT");
			Assert.NotNull(val1);
		}
	}
}
