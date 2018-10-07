//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2018 Dapplo
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
using Dapplo.Config.Tests.IniTests.Entities;
using Dapplo.Config.Tests.IniTests.Interfaces;
using Dapplo.Ini;
using Dapplo.Ini.Converters;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.IniTests
{
    public sealed class IniContainerTests
    {
        private const string Name = "Dapplo";
        private const string FirstName = "Robin";
        private const string TestValueForNonSerialized = "Hello!";

        public IniContainerTests(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
            StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
        }

        private async Task ConfigureMemoryStreamAsync(IniFileContainer iniFileContainer)
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
                .WithFixedDirectory("IniTestFiles")
                .BuildApplicationConfig();

            return new IniFileContainer(iniFileConfig, new []{ iniSection });
        }

        /// <summary>
        ///     This method tests that the initialization of the ini works.
        ///     Including the after load
        /// </summary>
        [Fact]
        public async Task TestIniAfterLoad()
        {
            var iniConfigTest = new IniConfigTestImpl();
            var iniContainer = CreateContainer("TestIniAfterLoad", iniConfigTest);

            iniConfigTest.OnLoad = x =>
            {
                if (!x.SomeValues.ContainsKey("dapplo"))
                {
                    x.SomeValues.Add("dapplo", 2015);
                }
            };
            await ConfigureMemoryStreamAsync(iniContainer);

            Assert.True(iniConfigTest.SomeValues.ContainsKey("dapplo"));
            Assert.True(iniConfigTest.SomeValues["dapplo"] == 2015);
        }

        /// <summary>
        ///     Test if the loading worked
        /// </summary>
        [Fact]
        public async Task TestIniFromFile()
        {
            var iniConfigTest = new IniConfigTestImpl();
            var iniContainer = CreateContainer("TestIniFromFile", iniConfigTest);
            await iniContainer.ReloadAsync();

            Assert.Equal(210u, iniConfigTest.Height);

            await iniContainer.WriteAsync();
        }

        [Fact]
        public async Task TestIniGeneral()
        {
            var iniConfigTest = new IniConfigTestImpl();
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
    }
}