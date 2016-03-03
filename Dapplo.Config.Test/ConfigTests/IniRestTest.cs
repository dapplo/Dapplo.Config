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

using Dapplo.Config.Converters;
using Dapplo.Config.Ini;
using Dapplo.Config.Test.ConfigTests.Interfaces;
using Dapplo.LogFacade;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Test.ConfigTests
{
	public class IniRestTest : IDisposable
	{
		private static readonly LogSource Log = new LogSource();
		private const string Name = "Dapplo";
		public IniRestTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);

			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}

		public void Dispose()
		{
			// Remove the IniConfig drom the IniConfig-store
			IniConfig.Delete("Dapplo", "dapplo");
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

		[Fact]
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
			Assert.Equal("RobinKrom", iniTest.Name);

			// Test get
			var readRobinKromUri = new Uri("dummy:///IniConfig/get/Dapplo/dapplo/Test/Name");
			var iniRestCommand = IniRest.ProcessRestUri(readRobinKromUri);

			Assert.True(iniRestCommand.Results.Count(x => x.PropertyName == "Name") == 1);
			Assert.Equal("RobinKrom", iniRestCommand.Results.First(x=> x.PropertyName == "Name").Value);

			// Test set multiple
			var changeValuesUri = new Uri("dummy:///IniConfig/set/Dapplo/dapplo/Test?Name=JimBean&FirstName=50");
			IniRest.ProcessRestUri(changeValuesUri);
			Assert.Equal("JimBean", iniTest.Name);
			Assert.Equal("50", iniTest.FirstName);

			// Test add to a dictionary
			var addDictionaryValueUri = new Uri("dummy:///IniConfig/add/Dapplo/dapplo/Test/SomeValues?Highlight=10&Stop=20");
			IniRest.ProcessRestUri(addDictionaryValueUri);
			Assert.True(iniTest.SomeValues.ContainsKey("Highlight"));
			Assert.True(iniTest.SomeValues.ContainsKey("Stop"));
			Log.Info().WriteLine($"Highlight = {iniTest.SomeValues["Highlight"]}");
			// Re-add, this should overwrite previous values
			IniRest.ProcessRestUri(addDictionaryValueUri);

			// Test adding to a list
			var addListValueUri = new Uri("dummy:///IniConfig/add/Dapplo/dapplo/Test/WindowCornerCutShape?20");
			IniRest.ProcessRestUri(addListValueUri);
			Assert.True(iniTest.WindowCornerCutShape.Contains(20));

			// Test remove from dictionary
			var removeSomeValuesUri = new Uri("dummy:///IniConfig/remove/Dapplo/dapplo/Test/SomeValues?Answer&Stop");
			IniRest.ProcessRestUri(removeSomeValuesUri);
			Assert.False(iniTest.SomeValues.ContainsKey("Answer"));
			Assert.False(iniTest.SomeValues.ContainsKey("Stop"));

			// Test remove from list
			var removeCutShapeUri = new Uri("dummy:///IniConfig/remove/Dapplo/dapplo/Test/WindowCornerCutShape?5&1");
			IniRest.ProcessRestUri(removeCutShapeUri);
            Assert.True(listToTest.First() != 5);
        }
	}
}