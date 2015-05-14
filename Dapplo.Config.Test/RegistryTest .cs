/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015 Robin Krom
 * 
 * For more information see: http://dapplo.net/
 * dapplo repositories are hosted on GitHub: https://github.com/dapplo
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 1 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test
{
	/// <summary>
	/// This test class tests the registry capabilities of the proxy
	/// </summary>
	[TestClass]
	public class RegistryTest
	{
		[TestMethod]
		public void TestRegistryRead()
		{
			var registryTest = ProxyBuilder.GetOrCreateProxy<IRegistryTest>().PropertyObject;
			var keys1 = registryTest.LMRun32;
			var keys2 = registryTest.LMRun64;
			var keys3 = registryTest.CURun32;
			var keys4 = registryTest.CURun64;

			// assume that SOME Run key in 32/64 LocalMachine/CurrentUser is set
			Assert.IsTrue(keys1.Count >= 1 || keys2.Count >= 1 || keys3.Count >= 1 || keys4.Count >= 1);
		}
	}
}