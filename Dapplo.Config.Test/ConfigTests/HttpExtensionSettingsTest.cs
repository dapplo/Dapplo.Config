/*
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

using Dapplo.Config.Ini;
using Dapplo.Config.Test.ConfigTests.Interfaces;
using Dapplo.HttpExtensions;
using Dapplo.HttpExtensions.Factory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace Dapplo.Config.Test.ConfigTests
{
	[TestClass]
	public class HttpExtensionSettingsTest
	{
		[TestMethod]
		public async Task TestHttpExtensionsDefaultReadWrite()
		{
			var iniConfig = new IniConfig("Dapplo", "dapplo.httpextensions");
			using (var testMemoryStream = new MemoryStream())
			{
				await IniConfig.Current.ReadFromStreamAsync(testMemoryStream).ConfigureAwait(false);
			}
			var httpConfiguration = await iniConfig.RegisterAndGetAsync<IHttpConfiguration>();
			Assert.IsNotNull(httpConfiguration);
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
