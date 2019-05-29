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

using System.Diagnostics.CodeAnalysis;
using Dapplo.Config.Language;
using Dapplo.Config.Tests.LanguageTests.Interfaces;

namespace Dapplo.Config.Tests.LanguageTests.Entities
{
    [SuppressMessage("ReSharper", "UnassignedGetOnlyAutoProperty")]
    internal class LanguageLoaderTestImpl : LanguageBase<ILanguageLoaderTest>, ILanguageLoaderTest
    {
        #region Implementation of ILanguageLoaderPartTest

        public string Ok2 { get; }

        #endregion

        #region Implementation of ILanguageLoaderTest

        public string Ok { get; set; }
        public string OnlydeDe { get; }
        public string OnlyenUs { get; }
        public string OnlynlNl { get; }
        public string TestValue { get; }

        #endregion
    }
}
