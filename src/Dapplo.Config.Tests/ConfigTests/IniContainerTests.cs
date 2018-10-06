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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Tests.ConfigTests.Entities;
using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.Ini;
using Dapplo.Ini.Converters;
using Dapplo.Ini.NewImpl;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigTests
{
    public sealed class IniContainerTests
    {
        private const string Name = "Dapplo";
        private const string FirstName = "Robin";
        private const string TestValueForNonSerialized = "Hello!";
        private readonly IniConfigTestImpl _iniConfigTest;

        private readonly IniFileContainer _iniFileContainer;

        public IniContainerTests(ITestOutputHelper testOutputHelper)
        {
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
            StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
            _iniConfigTest = new IniConfigTestImpl();
            _iniFileContainer = Create();
        }

        private async Task ConfigureMemoryStreamAsync()
        {
            using (var testMemoryStream = new MemoryStream())
            {
                await _iniFileContainer.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
            }
        }

        private IniFileContainer Create()
        {
            var iniFileConfig = IniFileConfigBuilder.Create()
                .WithApplicationName("Dapplo")
                .WithFilename("dapplo")
                .WithoutSaveOnExit()
                .BuildApplicationConfig();

            return new IniFileContainer(iniFileConfig, new []{ _iniConfigTest });
        }

        /// <summary>
        ///     This method tests that the initialization of the ini works.
        ///     Including the after load
        /// </summary>
        [Fact]
        public async Task TestIniAfterLoad()
        {
            var iniContainer = Create();

            _iniConfigTest.OnLoad = x =>
            {
                if (!x.SomeValues.ContainsKey("dapplo"))
                {
                    x.SomeValues.Add("dapplo", 2015);
                }
            };
            await ConfigureMemoryStreamAsync();

            Assert.True(_iniConfigTest.SomeValues.ContainsKey("dapplo"));
            Assert.True(_iniConfigTest.SomeValues["dapplo"] == 2015);
        }
    }
}