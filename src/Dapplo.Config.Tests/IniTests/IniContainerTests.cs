// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Tests.IniTests.Interfaces;
using Dapplo.Config.Ini;
using Dapplo.Config.Ini.Converters;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.IniTests
{

    public sealed class IniContainerTests
    {
        public IniContainerTests(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911f";
            StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
        }

        private async Task LoadFromMemoryStreamAsync(IniFileContainer iniFileContainer)
        {
            using (var testMemoryStream = new MemoryStream())
            {
                await iniFileContainer.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
            }
        }

        private IniFileContainer CreateContainer(string iniFileName, IIniSection iniSection)
        {
            var iniFileConfig = IniFileConfigBuilder.Create()
                .WithApplicationName("Dapplo")
                .WithFilename(iniFileName)
                .WithoutSaveOnExit()
                .WithFixedDirectory(@"IniTests\IniTestFiles")
                .BuildIniFileConfig();

            return new IniFileContainer(iniFileConfig, new []{ iniSection });
        }

        /// <summary>
        ///     This method tests that the initialization of the ini works.
        ///     Including the after load
        /// </summary>
        [Fact]
        public async Task TestIniAfterLoad()
        {
            var iniConfigTest = IniSection<IIniConfigTest>.Create();
            iniConfigTest.RegisterAfterLoad(x =>
            {
                if (!(x is IIniConfigTest iniConfig))
                {
                    return;
                }

                if (!iniConfig.SomeValues.ContainsKey("dapplo"))
                {
                    iniConfig.SomeValues.Add("dapplo", 2015);
                }
            });

            var iniContainer = CreateContainer("TestIniAfterLoad", iniConfigTest);

           
            await LoadFromMemoryStreamAsync(iniContainer);

            Assert.True(iniConfigTest.SomeValues.ContainsKey("dapplo"));
            Assert.True(iniConfigTest.SomeValues["dapplo"] == 2015);
        }

        /// <summary>
        ///     Test if the loading worked
        /// </summary>
        [Fact]
        public async Task TestIniFromFile()
        {
            var iniConfigTest = IniSection<IIniConfigTest>.Create();
            var iniContainer = CreateContainer("TestIniFromFile", iniConfigTest);
            await iniContainer.ReloadAsync();

            Assert.Equal(210u, iniConfigTest.Height);

            await iniContainer.WriteAsync();
        }

        [Fact]
        public async Task TestIniGeneral()
        {
            var iniConfigTest = IniSection<IIniConfigTest>.Create();
            var iniContainer = CreateContainer("TestIniGeneral", iniConfigTest);
            await iniContainer.ReloadAsync();

            //Assert.Contains(new Uri("http://1.dapplo.net"), iniTest.MyUris);

            Assert.Equal(185u, iniConfigTest.Height);
            iniConfigTest.Height++;
            Assert.Equal(186u, iniConfigTest.Height);
            iniConfigTest.Height = 185;
            Assert.Equal(185u, iniConfigTest.Height);
            Assert.Equal(16, iniConfigTest.PropertySize.Width);
            Assert.Equal(100, iniConfigTest.PropertyArea.Width);
            Assert.True(iniConfigTest.WindowCornerCutShape.Count > 0);
            Assert.Equal("It works!", iniConfigTest.SubValuewithDefault);
            Assert.Equal(IniConfigTestValues.Value2, iniConfigTest.TestWithEnum);
            iniConfigTest.RestoreToDefault("TestWithEnum");
            Assert.Equal(IniConfigTestValues.Value2, iniConfigTest.TestWithEnum);
            Assert.Equal(IniConfigTestValues.Value2, iniConfigTest.TestWithEnumSubValue);
        }

        [Fact]
        public void TestIniNoLoading_Defaults()
        {
            var iniConfigTest = IniSection<IIniConfigTest>.Create();
            Assert.Equal(IniConfigTestValues.Value2, iniConfigTest.TestWithEnum);
        }
    }
}