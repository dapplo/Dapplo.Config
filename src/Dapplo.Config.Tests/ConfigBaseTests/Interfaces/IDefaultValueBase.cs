// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dapplo.Config.Tests.ConfigBaseTests.Interfaces
{
	/// <summary>
	///     This is the interface under test
	/// </summary>
	public interface IDefaultValueBase : ISubValue
    {
		[DefaultValue(21)]
		int Age { get; set; }

		[DefaultValue("10,20,30")]
		IList<int> Ages { get; set; }

        [DefaultValue("http://1.dapplo.net,http://2.dapplo.net")]
        IList<Uri> MyUris { get; set; }
    }
}