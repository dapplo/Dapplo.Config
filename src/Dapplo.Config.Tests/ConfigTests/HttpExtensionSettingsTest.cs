// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Threading.Tasks;
using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.HttpExtensions;
using Dapplo.HttpExtensions.Factory;
using Dapplo.Config.Ini;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

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
                .BuildIniFileConfig();

            // Important to disable the auto-save, otherwise we get test issues
            var httpConfiguration = IniSection<IHttpConfiguration>.Create();

            var iniFileContainer = new IniFileContainer(iniFileConfig, new[] { httpConfiguration });
			using (var testMemoryStream = new MemoryStream())
			{
				await iniFileContainer.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
			}
			Assert.NotNull(httpConfiguration);
			using (var writeStream = new MemoryStream())
			{
				await iniFileContainer.WriteToStreamAsync(writeStream).ConfigureAwait(false);
				writeStream.Seek(0, SeekOrigin.Begin);
				await iniFileContainer.ReadFromStreamAsync(writeStream).ConfigureAwait(false);
				var behaviour = new HttpBehaviour
				{
					HttpSettings = httpConfiguration
				};
				behaviour.MakeCurrent();
                using var _ = HttpClientFactory.Create();
			}
		}
	}
}