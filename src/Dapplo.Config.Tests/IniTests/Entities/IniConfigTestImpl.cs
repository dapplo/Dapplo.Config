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

using AutoProperties;
using Dapplo.Config.Tests.IniTests.Interfaces;
using Dapplo.Ini;
using Dapplo.Windows.Common.Structs;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Dapplo.Config.Tests.IniTests.Entities
{
    public class IniConfigTestImpl : IniSectionBase<IIniConfigTest>, IIniConfigTest
    {
        public long Age { get; set; }
        public IDictionary<string, IList<int>> DictionaryOfLists { get; set; }
        public string FirstName { get; set; }
        public uint Height { get; set; }
        public IDictionary<string, Uri> ListOfUris { get; set; }
        public NativeSize MySize { get; set; }
        public string Name { get; set; }
        public string NotWritten { get; set; }
        public NativeRect PropertyArea { get; set; }
        public NativeSize PropertySize { get; set; }
        public IDictionary<string, int> SomeValues { get; set; }
        public IList<IniConfigTestValues> TestEnums { get; set; }
        public IniConfigTestValues TestWithEnum { get; set; }
        public IList<int> WindowCornerCutShape { get; set; }
        public Uri[] MyUris { get; set; }
        public string SubValue { get; set; }
        public string SubValuewithDefault { get; set; }
        public IniConfigTestValues TestWithEnumSubValue { get; set; }

        [InterceptIgnore]
        public Action<IIniConfigTest> OnLoad { get; set; }
        public override void AfterLoad()
        {
            OnLoad?.Invoke(this);
        }
    }
}
