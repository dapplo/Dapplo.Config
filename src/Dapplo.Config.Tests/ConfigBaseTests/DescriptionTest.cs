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

using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	public class DescriptionTest
	{
		public const string TestDescription = "Name of the person";
		private readonly IDescriptionTest _descriptionTest;

		public DescriptionTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
            _descriptionTest = DictionaryConfigurationBase<IDescriptionTest>.Create();
        }

		[Fact]
		public void TestDescriptionAttribute()
		{
			var description = _descriptionTest.DescriptionFor(nameof(IDescriptionTest.Name));
			Assert.Equal(TestDescription, description);
			description = _descriptionTest.DescriptionFor("Name");
			Assert.Equal(TestDescription, description);
		}
	}
}