// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.ComponentModel;
using Dapplo.Config.Ini;

namespace Dapplo.Config.Tests.RestTests.Interfaces
{
    /// <summary>
    ///     This is the interface under test
    /// </summary>
    [IniSection("Test")]
    [Description("Test Configuration")]
    public interface IIniRestTest : IIniSection
    {
        [DefaultValue("5,3,2,1,1")]
        IList<int> WindowCornerCutShape { get; set; }

        [Description("Firstname of the person")]
        string FirstName { get; set; }

        [Description("Name of the person")]
        string Name { get; set; }

        [Description("Here are some values")]
        IDictionary<string, int> SomeValues { get; set; }
    }
}