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

using Dapplo.Config.Language;
using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Dapplo.Config.Test
{
	/// <summary>
	/// This test class tests the transactional capabilities of the proxy
	/// </summary>
	[TestClass]
	public class LanguageLoaderTest
	{
		public const string Ok = "Ok";
		private LanguageLoader languageLoader;

		[TestInitialize]
		public void Initialize()
		{
			languageLoader = new LanguageLoader("Dapplo");
		}


		[TestCleanup]
		public void Cleanup()
		{
			LanguageLoader.Delete("Dapplo");
		}

		[TestMethod]
		[ExpectedException(typeof(NotSupportedException))]
		public async Task TestIllegalInterface()
		{
			await languageLoader.RegisterAndGetAsync<ILanguageLoaderFailTest>();
		}

		[TestMethod]
		public async Task TestModules()
		{
			// Make sure that the module (for testing) is available, we count all file-path which end with the filename 
			var count = languageLoader.Files.Count(file => file.EndsWith("language_mymodule-en-US.ini"));
			Assert.IsTrue(count > 0);

			var languageMyModule = await languageLoader.RegisterAndGetAsync<ILanguageLoaderMyModuleTest>();
			Assert.AreEqual("Some value", languageMyModule.ModuleSettings);
		}

		[TestMethod]
		public async Task TestIndexer()
		{
			var language = await languageLoader.RegisterAndGetAsync<ILanguageLoaderTest>();
            await languageLoader.ChangeLanguage("nl-NL");
			Assert.AreEqual("Afbreken", language["TestValue"]);
			// Test using the raw property name with the indexer
			Assert.AreEqual("Afbreken", languageLoader["test"]["test_value"]);
			Assert.AreEqual("cool", languageLoader["test"]["dapplo"]);
			Assert.IsTrue(languageLoader["test"].Keys().Contains("dapplo"));
		}

		[TestMethod]
		public async Task TestTranslations()
		{
			var language = await languageLoader.RegisterAndGetAsync<ILanguageLoaderTest>();
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("nl-NL"));
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("en-US"));
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("de-DE"));
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("sr-Cyrl-RS"));
			Assert.AreEqual("Nederlands (Nederland)", languageLoader.AvailableLanguages["nl-NL"]);
			await languageLoader.ChangeLanguage("en-US");
			Assert.AreEqual(Ok, language.Ok);
			Assert.AreEqual("Cancel", language.TestValue);
			Assert.AreEqual("BlubEN", language.OnlyenUS);
			Assert.AreNotEqual("BlubNL", language.OnlynlNL);
			Assert.AreNotEqual("BlubDE", language.OnlydeDE);
			await languageLoader.ChangeLanguage("nl-NL");
			Assert.AreEqual("Afbreken", language.TestValue);
			Assert.AreNotEqual("BlubEN", language.OnlyenUS);
			Assert.AreNotEqual("BlubDE", language.OnlydeDE);
			Assert.AreEqual("BlubNL", language.OnlynlNL);
			await languageLoader.ChangeLanguage("de-DE");
			Assert.AreEqual("BlubDE", language.OnlydeDE);
		}
	}
}