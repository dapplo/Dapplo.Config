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

using System.Collections.Generic;
using System.ComponentModel;
using Dapplo.Config.Ini;

#endregion

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