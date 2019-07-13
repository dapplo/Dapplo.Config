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

using System;
using System.Globalization;
using System.Threading;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigBaseTests
{
    /// <summary>
    ///     Test case to show how the default value works
    /// </summary>
    public class DefaultValueTest
    {
        private readonly IDefaultValueTest _defaultValueTest;

        public DefaultValueTest(ITestOutputHelper testOutputHelper)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            _defaultValueTest = DictionaryConfiguration<IDefaultValueTest>.Create();
        }

        [Fact]
        public void TestDefaultValue()
        {
            Assert.Equal(21, _defaultValueTest.Age);
            Assert.Equal(3, _defaultValueTest.Ages.Count);
            Assert.Equal(25, _defaultValueTest.Age2);
        }

        [Fact]
        public void TestDefaultValueOverwrite()
        {
            var defaultValueOverwriteTest = DictionaryConfiguration<IDefaultValueOverwriteTest>.Create();
            Assert.Equal(42, defaultValueOverwriteTest.Age);
        }

        [Fact]
        public void TestDefaultValueAtrribute()
        {
            var defaultValue = _defaultValueTest.DefaultValueFor(nameof(IDefaultValueTest.Age));
            Assert.Equal(21, defaultValue);
            defaultValue = _defaultValueTest.DefaultValueFor("Age");
            Assert.Equal(21, defaultValue);
        }

        [Fact]
        public void TestRestoreToDefaultValue()
        {
            _defaultValueTest.Age = 22;
            Assert.Equal(22, _defaultValueTest.Age);
            _defaultValueTest.RestoreToDefault(nameof(IDefaultValueTest.Age));
            Assert.Equal(21, _defaultValueTest.Age);
        }

        [Fact]
        public void TestUriArrayDefaultValue()
        {
            Assert.Contains(new Uri("http://1.dapplo.net"), _defaultValueTest.MyUris);
        }

        [Fact]
        public void TestDefaultValueWithError()
        {
            // TODO: Is this correct?
            IDefaultValueWithErrorTest value = DictionaryConfiguration<IDefaultValueWithErrorTest>.Create();
            // Should be the default enum value
            Assert.Equal(SimpleEnum.Value1, value.MyEnum);
        }
    }
}