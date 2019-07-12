//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
// 
//  For more information see: http://dapplo.net/
//  Dapplo repositories are hosted on GitHub: https://github.com/dapplo
// 
//  This file is part of Dapplo.Config
// 
//  Dapplo.Config is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  Dapplo.Config is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
// 
//  You should have a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Tests.IniTests.Interfaces;
using Dapplo.Config.Ini;
using Dapplo.Config.Ini.Converters;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;
using Dapplo.Config.Ini.Extensions;

#endregion

namespace Dapplo.Config.Tests.IniTests
{
    public sealed class IniContainerTests
    {
        public IniContainerTests(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
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
            var iniConfigTest = IniSectionBase<IIniConfigTest>.Create()
                .RegisterAfterLoad(x => {
                    if (!(x is IIniConfigTest iniConfig))
                    {
                        return;
                    }

                    if (!iniConfig.SomeValues.ContainsKey("dapplo"))
                    {
                        iniConfig.SomeValues.Add("dapplo", 2015);
                    }
                })
                .RegisterAfterLoad(x =>
                {
                    if (!(x is IIniConfigTest iniConfig))
                    {
                        return;
                    }

                    if (!iniConfig.SomeValues.ContainsKey("dapplo2"))
                    {
                        iniConfig.SomeValues.Add("dapplo2", 2016);
                    }
                });
            var iniContainer = CreateContainer("TestIniAfterLoad", iniConfigTest);

           
            await LoadFromMemoryStreamAsync(iniContainer);

            Assert.True(iniConfigTest.SomeValues.ContainsKey("dapplo"));
            Assert.True(iniConfigTest.SomeValues["dapplo"] == 2015);
            Assert.True(iniConfigTest.SomeValues["dapplo2"] == 2016);
        }

        /// <summary>
        ///     Test if the loading worked
        /// </summary>
        [Fact]
        public async Task TestIniFromFile()
        {
            var iniConfigTest = IniSectionBase<IIniConfigTest>.Create();
            var iniContainer = CreateContainer("TestIniFromFile", iniConfigTest);
            await iniContainer.ReloadAsync();

            Assert.Equal(210u, iniConfigTest.Height);

            await iniContainer.WriteAsync();
        }

        [Fact]
        public async Task TestIniGeneral()
        {
            var iniConfigTest = IniSectionBase<IIniConfigTest>.Create();
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
            var iniConfigTest = IniSectionBase<IIniConfigTest>.Create();
            Assert.Equal(IniConfigTestValues.Value2, iniConfigTest.TestWithEnum);
        }
    }
}