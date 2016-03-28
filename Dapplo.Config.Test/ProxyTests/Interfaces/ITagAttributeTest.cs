﻿//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2015-2016 Dapplo
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
//  You should have Config a copy of the GNU Lesser General Public License
//  along with Dapplo.Config. If not, see <http://www.gnu.org/licenses/lgpl.txt>.

#region using

using Dapplo.Config.Interceptor;
using Dapplo.Config.Interfaces;

#endregion

namespace Dapplo.Config.Test.ProxyTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	public interface ITagAttributeTest : ITagging<ITagAttributeTest>
	{
		[Tag("Expert")]
		int Age { get; set; }

		[Tag(TestTags.Tag2), Tag(TestTags.Tag1, "Robin")]
		string FirstName { get; set; }

		string Name { get; set; }
	}
}