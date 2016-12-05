//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016 Dapplo
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

using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.InterfaceImpl;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Dapplo.Registry.Implementation;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigTests
{
	/// <summary>
	///     This test class tests the registry capabilities of the proxy
	/// </summary>
	public class RegistryTest
	{
		public RegistryTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
		}

		[Fact]
		public void TestRegistryRead()
		{
			// TODO: Fix that we need this
			InterceptorFactory.RegisterExtension(typeof(RegistryExtension<>));

			var registryTest = InterceptorFactory.New<IRegistryTest>();

			// assume that the product name is set
			Assert.NotNull(registryTest.ProductName);

			Assert.Contains("CurrentVersion", registryTest.PathFor(x => x.ProductName));
		}
	}
}