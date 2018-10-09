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
using System.ComponentModel;
using System.Runtime.Serialization;
using Dapplo.Config.Ini;
using Dapplo.Config.Ini.Converters;
using Dapplo.Windows.Common.Structs;

#endregion

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