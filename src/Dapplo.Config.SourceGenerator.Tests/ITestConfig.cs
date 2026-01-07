// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.Config.Ini;

namespace Dapplo.Config.SourceGenerator.Tests
{
    /// <summary>
    /// Simple test configuration interface
    /// </summary>
    [IniSection("TestConfig")]
    [Description("Test Configuration for Source Generator")]
    public interface ITestConfig : IIniSection
    {
        [DefaultValue("Test")]
        string Name { get; set; }

        [DefaultValue(42)]
        int Age { get; set; }

        [DefaultValue(true)]
        bool IsEnabled { get; set; }
    }
}
