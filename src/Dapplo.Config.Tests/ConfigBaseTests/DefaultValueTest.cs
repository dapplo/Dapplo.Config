// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Threading;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

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