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

using Dapplo.Config.Ini;
using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Dapplo.Config.Test
{
	[TestClass]
	public class IniTest
	{
		private const string Name = "Dapplo";
		private const string FirstName = "Robin";
		private const string TestValueForNonSerialized = "Hello!";

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

			using (var testMemoryStream = new MemoryStream())
			{
				await iniConfig.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
				var iniTest = await iniConfig.RegisterAndGetAsync<IIniTest>().ConfigureAwait(false);
				Assert.IsTrue(iniTest.WindowCornerCutShape.Count > 0);
			}
		}

		[TestMethod]
		public async Task TestIniWriteRead()
		{
			var iniConfig = new IniConfig("Dapplo", "dapplo");

			var iniTest = await iniConfig.RegisterAndGetAsync<IIniTest>().ConfigureAwait(false);

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
		}
	}
}