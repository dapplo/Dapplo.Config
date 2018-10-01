//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2017 Dapplo
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

using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.HttpExtensions;
using Dapplo.HttpExtensions.Factory;
using Dapplo.Ini;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.ConfigTests
{
	public class HttpExtensionSettingsTest
	{
		public HttpExtensionSettingsTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
		}

		[Fact]
		public async Task TestHttpExtensionsDefaultReadWrite()
		{
            var iniFileConfig = IniFileConfigBuilder.Create()
                .WithApplicationName("Dapplo")
                .WithFilename("dapplo.httpextensions")
                .WithoutSaveOnExit()
                .BuildApplicationConfig();

            // Important to disable the auto-save, otherwise we get test issues
            var iniConfig = new IniConfig(iniFileConfig);
			using (var testMemoryStream = new MemoryStream())
			{
				await IniConfig.Current.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
			}
			var httpConfiguration = await iniConfig.RegisterAndGetAsync<IHttpConfiguration>();
			Assert.NotNull(httpConfiguration);
			using (var writeStream = new MemoryStream())
			{
				await iniConfig.WriteToStreamAsync(writeStream).ConfigureAwait(false);
				writeStream.Seek(0, SeekOrigin.Begin);
				await iniConfig.ReadFromStreamAsync(writeStream).ConfigureAwait(false);
				var behaviour = new HttpBehaviour
				{
					HttpSettings = httpConfiguration
				};
				behaviour.MakeCurrent();
				HttpClientFactory.Create();
			}
		}
	}
}