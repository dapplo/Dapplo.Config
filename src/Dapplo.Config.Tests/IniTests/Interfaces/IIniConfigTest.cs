// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Dapplo.Config.Ini;
using Dapplo.Config.Ini.Converters;
using Dapplo.Windows.Common.Structs;

namespace Dapplo.Config.Tests.IniTests.Interfaces
{
    public enum IniConfigTestValues
    {
        Value1,
        Value2
    }

    /// <summary>
    ///     This is the interface under test
    /// </summary>
    [IniSection("Test")]
    [Description("Test Configuration")]
    public interface IIniConfigTest : IIniConfigSubInterfaceTest, IIniSection
    {
        [DefaultValue(21)]
        [DataMember(EmitDefaultValue = true)]
        long Age { get; set; }

        [Description("Here are some cool values")]
        IDictionary<string, IList<int>> DictionaryOfLists { get; set; }

        [TypeConverter(typeof(StringEncryptionTypeConverter))]
        string FirstName { get; set; }

        [DefaultValue(185)]
        [DataMember(EmitDefaultValue = true)]
        uint Height { get; set; }

        [Description("The URIs for a test")]
        IDictionary<string, Uri> ListOfUris { get; set; }

        [DefaultValue("")]
        NativeSize MySize { get; set; }

        [Description("Name of the person")]
        string Name { get; set; }

        [IniPropertyBehavior(Read = false, Write = false)]
        string NotWritten { get; set; }

        [DefaultValue("16,16,100,100")]
        NativeRect PropertyArea { get; set; }

        [DefaultValue("16,16")]
        NativeSize PropertySize { get; set; }

        [Description("Here are some values")]
        IDictionary<string, int> SomeValues { get; set; }

        [Description("List of enums")]
        IList<IniConfigTestValues> TestEnums { get; set; }

        [Description("Test property for enums")]
        [DefaultValue(IniConfigTestValues.Value2)]
        IniConfigTestValues TestWithEnum { get; set; }

        [DefaultValue("5,3,2,1,1")]
        IList<int> WindowCornerCutShape { get; set; }

        [DefaultValue(new[] { "http://1.dapplo.net", "http://2.dapplo.net" })]
        Uri[] MyUris { get; set; }
    }
}