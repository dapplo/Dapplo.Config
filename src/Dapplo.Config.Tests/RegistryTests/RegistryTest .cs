// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.Registry;
using Dapplo.Config.Tests.RegistryTests.Interfaces;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Tests.RegistryTests
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
            IRegistryTest registryTest = RegistryBase<IRegistryTest>.Create();

			// assume that the product name is set
			Assert.NotNull(registryTest.ProductName);
			Assert.NotNull(registryTest.CuRun64);
            Assert.Contains("CurrentVersion", registryTest.PathFor(nameof(IRegistryTest.ProductName)));
		}
	}
}