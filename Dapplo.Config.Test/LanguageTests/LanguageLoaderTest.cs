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

using Dapplo.Config.Language;
using Dapplo.Config.Support;
using Dapplo.Config.Test.LanguageTests.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dapplo.Config.Test.LanguageTests
{
	/// <summary>
	/// This test class tests the transactional capabilities of the proxy
	/// </summary>
	[TestClass]
	public class LanguageLoaderTest
	{
		public const string Ok = "Ok";
		private LanguageLoader _languageLoader;

		[TestInitialize]
		public void Initialize()
		{
			_languageLoader = new LanguageLoader("Dapplo");
			_languageLoader.CorrectMissingTranslations();
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
			await _languageLoader.RegisterAndGetAsync<ILanguageLoaderFailTest>();
		}

		[TestMethod]
		public async Task TestModules()
		{
			// Make sure that the module (for testing) is available, we count all file-path which end with the filename 
			var count = _languageLoader.Files["en-US"].Count(file => file.EndsWith("language_mymodule-en-US.ini"));
			Assert.IsTrue(count > 0);

			var languageMyModule = await _languageLoader.RegisterAndGetAsync<ILanguageLoaderMyModuleTest>();
			Assert.AreEqual("Some value", languageMyModule.ModuleSettings);
		}

		[TestMethod]
		public async Task TestIndexer()
		{
			var language = await _languageLoader.RegisterAndGetAsync<ILanguageLoaderTest>();
			await _languageLoader.ChangeLanguage("nl-NL");
			Assert.AreEqual("Afbreken", language["TestValue"]);
			// Test using the raw property name with the indexer
			Assert.AreEqual("Afbreken", _languageLoader["test"]["test_value"]);
			Assert.AreEqual("cool", _languageLoader["test"]["dapplo"]);
			Assert.IsTrue(_languageLoader["test"].Keys().Contains("dapplo"));
		}

		[TestMethod]
		public async Task TestTranslations()
		{
			var language = await _languageLoader.RegisterAndGetAsync<ILanguageLoaderTest>();
			Assert.IsTrue(_languageLoader.AvailableLanguages.ContainsKey("nl-NL"));
			Assert.IsTrue(_languageLoader.AvailableLanguages.ContainsKey("en-US"));
			Assert.IsTrue(_languageLoader.AvailableLanguages.ContainsKey("de-DE"));
			Assert.IsTrue(_languageLoader.AvailableLanguages.ContainsKey("sr-Cyrl-RS"));
			Assert.AreEqual("Nederlands (Nederland)", _languageLoader.AvailableLanguages["nl-NL"]);
			await _languageLoader.ChangeLanguage("en-US");
			Assert.AreEqual(Ok, language.Ok);
			Assert.AreEqual("Cancel", language.TestValue);
			Assert.AreEqual("BlubEN", language.OnlyenUs);
			Assert.AreNotEqual("BlubNL", language.OnlynlNl);
			Assert.AreNotEqual("BlubDE", language.OnlydeDe);
			await _languageLoader.ChangeLanguage("nl-NL");
			Assert.AreEqual("Afbreken", language.TestValue);
			Assert.AreNotEqual("BlubEN", language.OnlyenUs);
			Assert.AreNotEqual("BlubDE", language.OnlydeDe);
			Assert.AreEqual("BlubNL", language.OnlynlNl);
			await _languageLoader.ChangeLanguage("de-DE");
			Assert.AreEqual("BlubDE", language.OnlydeDe);
		}

		[TestMethod]
		public async Task TestExtension()
		{
			ILanguageLoaderTest test = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			// This is actually what we want to test, extension method can work on null values :)
			var ok = test.DefaultTranslation(x => x.Ok);
			Assert.AreEqual("Ok", ok);

			test = await _languageLoader.RegisterAndGetAsync<ILanguageLoaderTest>();
			ok = test.TranslationOrDefault(x => x.Ok);
			Assert.AreEqual("Ok", ok);
		}
	}
}