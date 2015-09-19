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
using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Dapplo.Config.Ini
{
	[TestClass]
	public class IniConfigTest
	{
		private const string Name = "Dapplo";
		private const string FirstName = "Robin";
		private const string TestValueForNonSerialized = "Hello!";

		[TestCleanup]
		public void Remove()
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

		[TestMethod]
		public async Task TestIniInit()
		{
			var iniConfig = new IniConfig("Dapplo", "dapplo");

			iniConfig.AfterLoad<IIniConfigTest>((x) =>
			{
				if (!x.SomeValues.ContainsKey("dapplo"))
				{
					x.SomeValues.Add("dapplo", 2015);
				}
			});
			using (var testMemoryStream = new MemoryStream())
			{
				await iniConfig.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
			}
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			Assert.IsTrue(iniTest.Height == 185);
			Assert.IsTrue(iniTest.PropertySize.Width == 16);
			Assert.IsTrue(iniTest.PropertyArea.Width == 100);
			Assert.IsTrue(iniTest.WindowCornerCutShape.Count > 0);
			Assert.AreEqual("It works!", iniTest.SubValuewithDefault);
			Assert.IsTrue(iniTest.SomeValues.ContainsKey("dapplo"));

			// Test ini value retrieval, by checking the Type and return value
			var iniValue = iniTest["WindowCornerCutShape"];
			Assert.IsTrue(iniValue.ValueType == typeof(IList<int>));
			Assert.IsTrue(((IList<int>)iniValue.Value).Count > 0);

			// Test try get
			IIniSection section;
			Assert.IsTrue(iniConfig.TryGet("Test", out section));
			IniValue tryGetValue;
			Assert.IsTrue(section.TryGetIniValue("WindowCornerCutShape", out tryGetValue));
			Assert.IsTrue(((IList<int>)tryGetValue.Value).Count > 0);
			Assert.IsFalse(section.TryGetIniValue("DoesNotExist", out tryGetValue));

			// Check second get, should have same value
			var iniTest2 = iniConfig.Get<IIniConfigTest>();
			Assert.IsTrue(iniTest2.WindowCornerCutShape.Count > 0);
			Assert.IsTrue(iniTest2.SomeValues.ContainsKey("dapplo"));

			// Test static Current and the static get
			var iniTest3 = IniConfig.Current.Get<IIniConfigTest>();
			Assert.IsTrue(iniTest2.WindowCornerCutShape.Count > 0);
			Assert.IsTrue(iniTest2.SomeValues.ContainsKey("dapplo"));

			// Test indexers
			Assert.IsTrue(iniConfig.SectionNames.Contains("Test"));
			var iniTest4 = iniConfig["Test"];
			Assert.AreEqual("It works!", iniTest4["SubValuewithDefault"].Value);
		}

		[TestMethod]
		public async Task TestIniWriteRead()
		{
			var iniConfig = new IniConfig("Dapplo", "dapplo");
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);

			// Change some values
			iniTest.Name = Name;
			iniTest.FirstName = FirstName;

			// This value should not be written to the file
			iniTest.NotWritten = "Whatever";

			// Dictionary test
			iniTest.SomeValues.Add("One", 1);

			// Some "random" value that needs to be there again after reading.
			long ticks = DateTimeOffset.Now.UtcTicks;
			iniTest.Age = ticks;
			using (var writeStream = new MemoryStream())
			{
				await iniConfig.WriteToStreamAsync(writeStream).ConfigureAwait(false);
				//await iniConfig.WriteAsync().ConfigureAwait(false);

				// Set the not written value to a testable value, this should not be read (and overwritten) by reading the ini file.
				iniTest.NotWritten = TestValueForNonSerialized;

				// Make sure Age is set to some value, so we can see that it is re-read
				iniTest.Age = 2;

				// Test reading
				writeStream.Seek(0, SeekOrigin.Begin);
				await iniConfig.ReadFromStreamAsync(writeStream).ConfigureAwait(false);
				//await iniConfig.ReloadAsync(false).ConfigureAwait(false);

				Assert.IsTrue(iniTest.SomeValues.ContainsKey("One"));
				Assert.AreEqual(Name, iniTest.Name);
				Assert.AreEqual(FirstName, iniTest.FirstName);
				Assert.AreEqual(ticks, iniTest.Age);
				Assert.AreEqual(TestValueForNonSerialized, iniTest.NotWritten);
			}

			// Check second get, should have same value
			var iniTest2 = iniConfig.Get<IIniConfigTest>();
			Assert.AreEqual(TestValueForNonSerialized, iniTest2.NotWritten);
		}

		[TestMethod]
		public async Task TestIniStringConvertion()
		{
			var iniConfig = new IniConfig("Dapplo", "dapplo");
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			var iniValue = iniTest["WindowCornerCutShape"];
			iniValue.ToString();

		}
	}
}