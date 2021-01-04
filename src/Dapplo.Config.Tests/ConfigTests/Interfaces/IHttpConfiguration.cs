// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel;
using Dapplo.HttpExtensions;
using Dapplo.Config.Ini;

namespace Dapplo.Config.Tests.ConfigTests.Interfaces
{
	/// <summary>
	///     Testing interface for storing the IHttpSettings from the Dapplo.HttpExtensions nuget package
	/// </summary>
	[IniSection("Http")]
	[Description("Test Configuration")]
	public interface IHttpConfiguration : IHttpSettings, IIniSection
	{
	}
}