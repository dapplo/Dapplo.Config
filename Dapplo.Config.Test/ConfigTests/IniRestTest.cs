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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapplo.Config.Converters;
using Dapplo.Config.Ini;
using Dapplo.Config.Test.ConfigTests.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test.ConfigTests
{
	[TestClass]
	public class IniRestTest
	{
		private const string Name = "Dapplo";

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

		/// <summary>
		/// Create an ini-config, but delete it first
		/// </summary>
		/// <returns></returns>
		private IniConfig Create()
		{
			IniConfig.Delete("Dapplo", "dapplo");
			return new IniConfig("Dapplo", "dapplo");
		}

		[TestMethod]
		public async Task TestIniRest()
		{
			var iniConfig = await InitializeAsync();
			var iniTest = await iniConfig.RegisterAndGetAsync<IIniConfigTest>().ConfigureAwait(false);
			var listToTest = iniTest.WindowCornerCutShape;
			iniTest.Name = Name;
			iniTest.SomeValues.Add("Answer", 42);

			// Test set
			var changeNameToRobinKromUri = new Uri("dummy:///IniConfig/set/Dapplo/dapplo/Test/Name/RobinKrom");
			IniRest.ProcessRestUri(changeNameToRobinKromUri);
			Assert.AreEqual("RobinKrom", iniTest.Name);

			// Test get
			var readRobinKromUri = new Uri("dummy:///IniConfig/get/Dapplo/dapplo/Test/Name/RobinKrom");
			var iniRestCommand = IniRest.ProcessRestUri(readRobinKromUri);

			Assert.IsTrue(iniRestCommand.Results.Count(x => x.PropertyName == "Name") == 1);
			Assert.AreEqual("RobinKrom", iniRestCommand.Results.First(x=> x.PropertyName == "Name").Value);

			// Test set multiple
			var changeValuesUri = new Uri("dummy:///IniConfig/set/Dapplo/dapplo/Test?Name=JimBean&FirstName=50");
			IniRest.ProcessRestUri(changeValuesUri);
			Assert.AreEqual("JimBean", iniTest.Name);
			Assert.AreEqual("50", iniTest.FirstName);

			// Test add
			var addValueUri = new Uri("dummy:///IniConfig/add/Dapplo/dapplo/Test/SomeValues?Highlight=10&Stop=20");
			var addValueResult = IniRest.ProcessRestUri(addValueUri);
			Assert.IsTrue(iniTest.SomeValues.ContainsKey("Highlight"));
			Assert.IsTrue(iniTest.SomeValues.ContainsKey("Stop"));
			Debug.WriteLine($"Highlight = {iniTest.SomeValues["Highlight"]}");

			// Test remove from dictionary
			var removeSomeValuesUri = new Uri("dummy:///IniConfig/remove/Dapplo/dapplo/Test/SomeValues?Answer&Stop");
			IniRest.ProcessRestUri(removeSomeValuesUri);
			Assert.IsFalse(iniTest.SomeValues.ContainsKey("Answer"));
			Assert.IsFalse(iniTest.SomeValues.ContainsKey("Stop"));

			// Test remove from list
			
			var removeCutShapeUri = new Uri("dummy:///IniConfig/remove/Dapplo/dapplo/Test/WindowCornerCutShape?5&1");
			IniRest.ProcessRestUri(removeCutShapeUri);
            Assert.IsTrue(listToTest.First() != 5);
        }
	}
}