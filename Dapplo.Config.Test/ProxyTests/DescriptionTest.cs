/*
	Dapplo - building blocks for desktop applications
	Copyright (C) 2015-2016 Dapplo

	For more information see: http://dapplo.net/
	Dapplo repositories are hosted on GitHub: https://github.com/dapplo

	This file is part of Dapplo.Config

	Dapplo.Config is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	Dapplo.Config is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

	You should have Config a copy of the GNU Lesser General Public License
	along with Dapplo.HttpExtensions. If not, see <http://www.gnu.org/licenses/lgpl.txt>.
 */

using Dapplo.Config.Test.ProxyTests.Interfaces;
using Dapplo.LogFacade;
using Xunit;
using Xunit.Abstractions;

namespace Dapplo.Config.Test.ProxyTests
{
	public class DescriptionTest
	{
		private IPropertyProxy<IDescriptionTest> _propertyProxy;
		public const string TestDescription = "Name of the person";

		public DescriptionTest(ITestOutputHelper testOutputHelper)
		{
			XUnitLogger.RegisterLogger(testOutputHelper, LogLevel.Verbose);
			_propertyProxy = ProxyBuilder.CreateProxy<IDescriptionTest>();
		}

		[Fact]
		public void TestDescriptionAttribute()
		{
			string description = _propertyProxy.PropertyObject.DescriptionFor(x => x.Name);
			Assert.Equal(description, TestDescription);
			description = _propertyProxy.PropertyObject.DescriptionFor("Name");
			Assert.Equal(description, TestDescription);
		}
	}
}