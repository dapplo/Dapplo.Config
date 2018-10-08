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

using System.Collections.Generic;
using Dapplo.Config.Ini;
using Dapplo.Config.Tests.RestTests.Interfaces;

namespace Dapplo.Config.Tests.RestTests.Entities
{
    public class IniRestTestImpl : IniSectionBase<IIniRestTest>, IIniRestTest
    {
        public IList<int> WindowCornerCutShape { get; set; }
        public IDictionary<string, int> SomeValues { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
    }
}
