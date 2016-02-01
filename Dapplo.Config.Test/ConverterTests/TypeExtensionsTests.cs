using Dapplo.Config.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Dapplo.Config.Test.ConverterTests
{
	[TestClass]
	public class TypeExtensionsTests
	{
		[TestMethod]
		public void TestConvertOrCastValueToType_Uri()
		{
			var testUri = new Uri("http://test.com/dapplo?name=config");

			var stringUri = typeof(string).ConvertOrCastValueToType(testUri, convertFrom: false);
			Assert.AreEqual(testUri.AbsoluteUri, stringUri);

			var convertedUri = typeof(Uri).ConvertOrCastValueToType(stringUri) as Uri;
			Assert.AreEqual(testUri, convertedUri);
		}

		[TestMethod]
		public void TestConvertOrCastValueToType_DictionaryWithUris()
		{
			var testUri = new Uri("http://test.com/dapplo?name=config");

			var testValues = new Dictionary<string, Uri>();
			testValues.Add("value1", testUri);

			var stringDictionary = typeof(IDictionary<string, string>).ConvertOrCastValueToType(testValues, convertFrom: false) as IDictionary<string, string>;
			Assert.AreEqual(testValues["value1"].AbsoluteUri, stringDictionary["value1"]);

			var uriDictionary = typeof(Dictionary<string, Uri>).ConvertOrCastValueToType(stringDictionary) as IDictionary<string, Uri>;
			Assert.AreEqual(testValues["value1"].AbsoluteUri, uriDictionary["value1"].AbsoluteUri);
		}
	}
}
