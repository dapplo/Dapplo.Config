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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Converters;
using Dapplo.Config.Ini;
using Dapplo.Config.Test.ConfigTests.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;

namespace Dapplo.Config.Test.ConfigTests
{
	[TestClass]
	public class IniConfigTest
	{
		private const string Name = "Dapplo";
		private const string FirstName = "Robin";
		private const string TestValueForNonSerialized = "Hello!";

		[TestCleanup]
		public void Cleanup()
		{
			// Remove the IniConfig drom the IniConfig-store
			IniConfig.Delete("Dapplo", "dapplo");
		}

		[ClassInitialize]
		public static void InitializeClass(TestContext textContext)
		{
			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}

		private async Task<IniConfig> InitializeAsync()
		{
			var iniConfig = Create();
			await ConfigureMemoryStreamAsync();
			return iniConfig;
		}

		private async Task ConfigureMemoryStreamAsync()
		{
			using (var testMemoryStream = new MemoryStream())
			{
				await IniConfig.Current.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
			}
		}

		private IniConfig Create()
		{
			return new IniConfig("Dapplo", "dapplo");
		}

		/// <summary>
		/// This method tests that the initialization of the ini works.
		/// Including the after load
		/// </summary>
		[TestMethod]
		public async Task TestIniAfterLoad()
		{
			var iniConfig = Create();
			iniConfig.AfterLoad<IIniConfigTest>(x =>
			{
				if (!x.SomeValues.ContainsKey("dapplo"))
				{
					x.SomeValues.Add("dapplo", 2015);
				}
		
			});
			await ConfigureMemoryStreamAsync();

			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			Assert.IsTrue(iniTest.SomeValues.ContainsKey("dapplo"));
			Assert.IsTrue(iniTest.SomeValues["dapplo"] == 2015);
		}

		[TestMethod]
		public async Task TestIniGeneral()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);

			Assert.IsTrue(iniTest.Height == 185);
			iniTest.Height++;
			Assert.IsTrue(iniTest.Height == 186);
			iniTest.Height = 185;
			Assert.IsTrue(iniTest.Height == 185);
			Assert.IsTrue(iniTest.PropertySize.Width == 16);
			Assert.IsTrue(iniTest.PropertyArea.Width == 100);
			Assert.IsTrue(iniTest.WindowCornerCutShape.Count > 0);
			Assert.AreEqual("It works!", iniTest.SubValuewithDefault);
			Assert.AreEqual(IniConfigTestEnum.Value2, iniTest.TestWithEnum);
			iniTest.RestoreToDefault("TestWithEnum");
            Assert.AreEqual(IniConfigTestEnum.Value2, iniTest.TestWithEnum);
			Assert.AreEqual(IniConfigTestEnum.Value2, iniTest.TestWithEnumSubValue);
		}

		[TestMethod]
		public async Task TestIniIndexAccess()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test ini value retrieval, by checking the Type and return value
			var iniValue = iniTest["WindowCornerCutShape"];
			Assert.IsTrue(iniValue.ValueType == typeof(IList<int>));
			Assert.IsTrue(((IList<int>)iniValue.Value).Count > 0);
		}

		[TestMethod]
		public async Task TestIniSectionTryGet()
		{
			var iniConfig = await InitializeAsync();
			await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test try get
			IIniSection section;
			Assert.IsTrue(iniConfig.TryGet("Test", out section));
			IniValue tryGetValue;
			Assert.IsTrue(section.TryGetIniValue("WindowCornerCutShape", out tryGetValue));
			Assert.IsTrue(((IList<int>)tryGetValue.Value).Count > 0);
			Assert.IsFalse(section.TryGetIniValue("DoesNotExist", out tryGetValue));
		}

		[TestMethod]
		public async Task TestIniConfigIndexConvertion()
		{
			var iniConfig = await InitializeAsync();
			await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test indexers
			Assert.IsTrue(iniConfig.SectionNames.Contains("Test"));
			var iniTest = (IIniConfigTest)iniConfig["Test"];

			// Set value with wrong type (but valid value)
			iniConfig["Test"]["Height"].Value = "100";

			Assert.AreEqual((uint)100, iniTest.Height);
        }

		[TestMethod]
		public async Task TestIniWrongEnumDefault()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigWrongEnumTest>().ConfigureAwait(false);
			Assert.AreEqual(IniConfigTestEnum.Value1, iniTest.TestWithFalseEnum);
		}

		[TestMethod]
		public async Task TestIniConfigIndex()
		{
			var iniConfig = await InitializeAsync();
			await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			// Test indexers
			Assert.IsTrue(iniConfig.SectionNames.Contains("Test"));
			var iniTest = iniConfig["Test"];
			Assert.AreEqual("It works!", iniTest["SubValuewithDefault"].Value);
		}

		[TestMethod]
		public async Task TestIniWriteRead()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);

			iniTest.WindowCornerCutShape.Add(100);
			iniTest.DictionaryOfLists.Add("firstValue", new List<int>() { 10, 20 });
			// Change some values
			iniTest.Name = Name;
			iniTest.FirstName = FirstName;

			Assert.AreEqual(Size.Empty, iniTest.MySize);
			// This value should not be written to the file
			iniTest.NotWritten = "Whatever";

			// Dictionary test
			iniTest.SomeValues.Add("One", 1);
			iniTest.TestEnums.Add(IniConfigTestEnum.Value2);
			// Enum test
			iniTest.TestWithEnum = IniConfigTestEnum.Value1;
			iniTest.TestWithEnumSubValue = IniConfigTestEnum.Value1;

			// Some "random" value that needs to be there again after reading.
			long ticks = DateTimeOffset.Now.UtcTicks;
			iniTest.Age = ticks;
            var heightBefore = ++iniTest.Height;
			await iniConfig.WriteAsync().ConfigureAwait(false);
			using (var writeStream = new MemoryStream())
			{
				await iniConfig.WriteToStreamAsync(writeStream).ConfigureAwait(false);

				//await iniConfig.WriteAsync().ConfigureAwait(false);

				// Set the not written value to a testable value, this should not be read (and overwritten) by reading the ini file.
				iniTest.NotWritten = TestValueForNonSerialized;

				// Make sure Age is set to some value, so we can see that it is re-read
				iniTest.Age = 2;

				// Make sure we change the value, to see if it's overwritten
                iniTest.Height++;

				// Test reading
				writeStream.Seek(0, SeekOrigin.Begin);
				await iniConfig.ReadFromStreamAsync(writeStream).ConfigureAwait(false);
				//await iniConfig.ReloadAsync(false).ConfigureAwait(false);
                Assert.IsTrue(iniTest.SomeValues.ContainsKey("One"));
				Assert.IsTrue(iniTest.WindowCornerCutShape.Contains(100));
				// check if the dictionary of lists also has all values again
				Assert.IsTrue(iniTest.DictionaryOfLists.ContainsKey("firstValue"));
				Assert.IsTrue(iniTest.DictionaryOfLists["firstValue"].Count == 2);

				// Rest of simple tests
				Assert.AreEqual(Name, iniTest.Name);
				Assert.AreEqual(FirstName, iniTest.FirstName);
				Assert.AreEqual(ticks, iniTest.Age);
				Assert.AreEqual(TestValueForNonSerialized, iniTest.NotWritten);
				Assert.AreEqual(IniConfigTestEnum.Value1, iniTest.TestWithEnum);
				Assert.AreEqual(IniConfigTestEnum.Value1, iniTest.TestWithEnumSubValue);
				Assert.AreEqual(heightBefore, iniTest.Height);
				Assert.IsTrue(iniTest.TestEnums.Contains(IniConfigTestEnum.Value2));
			}

			// Check second get, should have same value
			var iniTest2 = iniConfig.Get<IIniConfigTest>();
			Assert.AreEqual(TestValueForNonSerialized, iniTest2.NotWritten);
		}
	}
}