﻿/*
 * dapplo - building blocks for desktop applications
 * Copyright (C) 2015-2016 Dapplo
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

using Dapplo.Config.Converters;
using Dapplo.LogFacade;
using Dapplo.LogFacade.Loggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test
{
	/// <summary>
	/// This initializes the logger and some other stuff for all tests
	/// </summary>
	[TestClass]
	public class BeforeAllTests
	{
		[AssemblyInitialize]
		public static void Configure(TestContext context)
		{
			LogSettings.Logger = new TraceLogger { Level = LogLevel.Verbose };
			StringEncryptionTypeConverter.RgbIv = "fjr84hF49gp3911fFFg";
			StringEncryptionTypeConverter.RgbKey = "ljew3lJfrS0rlddlfeelOekfekcvbAwE";
		}
	}
}
