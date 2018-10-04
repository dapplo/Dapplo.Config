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

using Dapplo.Config.Tests.ConfigBaseTests.Entities;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigBaseTests
{
	/// <summary>
	///     Test case to show how the HasChanges works
	/// </summary>
	public class HasChangesTests
	{
		private readonly IHasChangesTest _hasChangesTest;

		public HasChangesTests(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
			_hasChangesTest = new HasChangesImpl();
		}

		[Fact]
		public void TestHasChanges()
		{
			_hasChangesTest.SayMyName = "Robin";
			Assert.True(_hasChangesTest.HasChanges());
			Assert.True(_hasChangesTest.IsChanged(nameof(_hasChangesTest.SayMyName)));
			Assert.True(_hasChangesTest.IsChanged(nameof(IHasChangesTest.SayMyName)));
			Assert.True(_hasChangesTest.Changes().Contains(nameof(_hasChangesTest.SayMyName)));
			_hasChangesTest.ResetHasChanges();
			Assert.False(_hasChangesTest.HasChanges());
			Assert.False(_hasChangesTest.IsChanged(nameof(_hasChangesTest.SayMyName)));
			Assert.False(_hasChangesTest.IsChanged(nameof(IHasChangesTest.SayMyName)));
			Assert.False(_hasChangesTest.Changes().Contains(nameof(_hasChangesTest.SayMyName)));
		}
	}
}