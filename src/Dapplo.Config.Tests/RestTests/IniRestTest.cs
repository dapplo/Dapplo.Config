// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Dapplo.Config.Ini;
using Dapplo.Config.Ini.Converters;
using Dapplo.Config.Ini.Rest;
using Dapplo.Config.Tests.RestTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.RestTests
{
	public class IniRestTest 
	{
		private const string Name = "Dapplo";
		private static readonly LogSource Log = new LogSource();

		public IniRestTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);

			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911f";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}

		private IniFileContainer CreateContainer(string iniFileName, IIniSection iniSection)
		{
			var iniFileConfig = IniFileConfigBuilder.Create()
				.WithApplicationName("Dapplo")
				.WithFilename(iniFileName)
				.WithoutSaveOnExit()
				.WithFixedDirectory(@"RestTests\IniTestFiles")
				.BuildIniFileConfig();

			return new IniFileContainer(iniFileConfig, new[] { iniSection });
		}

		[Fact]
		public async Task TestIniRest()
		{
			var iniTest = IniSection<IIniRestTest>.Create();
			var iniContainer = CreateContainer("TestIniRest", iniTest);

			iniTest.Name = Name;
			iniTest.SomeValues.Add("Answer", 42);

			// Test set
			var changeNameToRobinKromUri = new Uri("dummy:///IniConfig/set/Dapplo/dapplo/Test/Name/RobinKrom");
			IniRest.ProcessRestUri(changeNameToRobinKromUri, iniContainer);
			Assert.Equal("RobinKrom", iniTest.Name);

			// Test get
			var readRobinKromUri = new Uri("dummy:///IniConfig/get/Dapplo/dapplo/Test/Name");
			var iniRestCommand = IniRest.ProcessRestUri(readRobinKromUri, iniContainer);

			Assert.True(iniRestCommand.Results.Count(x => x.PropertyName == "Name") == 1);
			Assert.Equal("RobinKrom", iniRestCommand.Results.First(x => x.PropertyName == "Name").Value);

            // Test set multiple
			Assert.NotEqual("JimBean", iniTest.Name);
			Assert.NotEqual("50", iniTest.FirstName);
			var changeValuesUri = new Uri("dummy:///IniConfig/set/Dapplo/dapplo/Test?Name=JimBean&FirstName=50");
			IniRest.ProcessRestUri(changeValuesUri, iniContainer);
            Assert.Equal("JimBean", iniTest.Name);
			Assert.Equal("50", iniTest.FirstName);

            // Test add to a dictionary
			Assert.False(iniTest.SomeValues.ContainsKey("Highlight"));
			Assert.False(iniTest.SomeValues.ContainsKey("Stop"));
			var addDictionaryValueUri = new Uri("dummy:///IniConfig/add/Dapplo/dapplo/Test/SomeValues?Highlight=10&Stop=20");
			IniRest.ProcessRestUri(addDictionaryValueUri, iniContainer);
            Assert.True(iniTest.SomeValues.ContainsKey("Highlight"));
			Assert.True(iniTest.SomeValues.ContainsKey("Stop"));
			Log.Info().WriteLine($"Highlight = {iniTest.SomeValues["Highlight"]}");
			// Re-add, this should overwrite previous values
			IniRest.ProcessRestUri(addDictionaryValueUri, iniContainer);

            // Test adding to a list
			Assert.False(iniTest.WindowCornerCutShape.Contains(20));
            var addListValueUri = new Uri("dummy:///IniConfig/add/Dapplo/dapplo/Test/WindowCornerCutShape?20");
			IniRest.ProcessRestUri(addListValueUri, iniContainer);
            Assert.True(iniTest.WindowCornerCutShape.Contains(20));

            // Test remove from dictionary
			Assert.True(iniTest.SomeValues.ContainsKey("Answer"));
			Assert.True(iniTest.SomeValues.ContainsKey("Stop"));
            var removeSomeValuesUri = new Uri("dummy:///IniConfig/remove/Dapplo/dapplo/Test/SomeValues?Answer&Stop");
			IniRest.ProcessRestUri(removeSomeValuesUri, iniContainer);
            Assert.False(iniTest.SomeValues.ContainsKey("Answer"));
			Assert.False(iniTest.SomeValues.ContainsKey("Stop"));

            // Test remove from list
			var listToTest = iniTest.WindowCornerCutShape;
			Assert.Equal(5, listToTest.First());
			var removeCutShapeUri = new Uri("dummy:///IniConfig/remove/Dapplo/dapplo/Test/WindowCornerCutShape?5&1");
			IniRest.ProcessRestUri(removeCutShapeUri, iniContainer);
			Assert.NotEqual(5, listToTest.First());

			await iniContainer.WriteAsync();
		}
	}
}