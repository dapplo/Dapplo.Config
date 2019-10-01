//  Dapplo - building blocks for desktop applications
//  Copyright (C) 2016-2019 Dapplo
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

using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dapplo.Config.Tests.LanguageTests.Interfaces;
using Dapplo.Config.Language;
using Dapplo.Log;
using Dapplo.Log.XUnit;
using Xunit;
using Xunit.Abstractions;

#endregion

namespace Dapplo.Config.Tests.LanguageTests
{
	/// <summary>
	///     This test class tests the language container
	/// </summary>
	public class LanguageContainerTest
    {
		public const string Ok = "Ok";
		private static readonly LogSource Log = new LogSource();

		public LanguageContainerTest(ITestOutputHelper testOutputHelper)
		{
			LogSettings.RegisterDefaultLogger<XUnitLogger>(LogLevels.Verbose, testOutputHelper);
		}

	    private async Task<LanguageContainer> CreateContainer(ILanguage language)
	    {
		    var languageConfig = LanguageConfigBuilder.Create()
			    .WithApplicationName("Dapplo")
			    .WithSpecificDirectories(@"LanguageTests\LanguageTestFiles")
			    .BuildLanguageConfig();

		    var result = new LanguageContainer(languageConfig, new[] { language });
		    await result.ReloadAsync().ConfigureAwait(false);

            result.CorrectMissingTranslations();
		    return result;
	    }


        [Fact]
		public async Task Test_LanguageChanged()
        {
	        var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            using var languageContainer = await CreateContainer(languageLoaderTest);
			var changed = false;
			var eventRegistration = languageLoaderTest.OnLanguageChanged(sender => changed = true);

	        await languageContainer.ChangeLanguageAsync("nl-NL");
			eventRegistration.Dispose();
			Assert.True(changed);
		}

	    [Fact]
	    public async Task Test_Enumerable()
	    {
		    var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            using var languageContainer = await CreateContainer(languageLoaderTest);
            var prefixes = languageContainer.Select(l => l.PrefixName()).ToList();
            Assert.Contains("Test", prefixes);
	        // No module support yet - Assert.Contains("MyModule", prefixes);
        }

        [Fact]
		public async Task TestExtension()
		{
			var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            using var _ = await CreateContainer(languageLoaderTest);

            ILanguageLoaderTest test = null;
			// ReSharper disable once ExpressionIsAlwaysNull
			// This is actually what we want to test, extension method can work on null values :)
			var ok = test.DefaultTranslation(x => x.Ok);
			Assert.Equal("Ok", ok);

			ok = languageLoaderTest.TranslationOrDefault(x => x.Ok);
			Assert.Equal("Ok", ok);

			ok = languageLoaderTest.TranslationOrDefault(x => x.Ok);
			Assert.Equal("Ok", ok);
		}

		[Fact]
		public async Task TestIndexer()
		{
			var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            using var languageContainer = await CreateContainer(languageLoaderTest);
			await languageContainer.ChangeLanguageAsync("nl-NL");
			var afbreken = languageLoaderTest["TestValue"];
			Assert.Equal("Afbreken", afbreken);
			// Test using the raw property name with the indexer
			Assert.Equal("Afbreken", languageContainer["test"]["test_value"]);
			Assert.Equal("cool", languageContainer["test"]["dapplo"]);
			Assert.Equal("cool", languageLoaderTest["dapplo"]);
			Assert.Equal("cool", languageLoaderTest["dapplo_"]);
			Assert.Contains("dapplo", languageLoaderTest.PropertyFreeKeys());
			Assert.Contains("dapplo_", languageLoaderTest.PropertyFreeKeys());
            // Test if translations without matching properties are available
            Assert.Contains("dapplo", languageContainer["test"].Keys());
		}

		[Fact]
		public async Task TestINotifyPropertyChanged()
		{
			var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            var languageContainer = await CreateContainer(languageLoaderTest);
			var hasChanges = false;
			var propChanged = new PropertyChangedEventHandler((sender, eventArgs) =>
			{
				Log.Debug().WriteLine($"Change for {eventArgs.PropertyName}");
				hasChanges = true;
			});

            // Test event handler
			languageLoaderTest.PropertyChanged += propChanged;

			await languageContainer.ChangeLanguageAsync("nl-NL").ConfigureAwait(false);

			// Make sure the events are handled
			await Task.Yield();
			Assert.True(hasChanges);
		}

		[Fact]
		public async Task TestLanguagePart()
		{
			var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            using var _ = await CreateContainer(languageLoaderTest);

			var partType = (ILanguageLoaderPartTest) languageLoaderTest;
			Assert.NotNull(partType);
			Assert.Equal("Ok", partType.Ok2);
		}

		[Fact]
		public async Task TestModules()
		{
			ILanguageLoaderMyModuleTest loaderMyModuleTest = Language<ILanguageLoaderMyModuleTest>.Create();
			using var languageContainer = await CreateContainer(loaderMyModuleTest);
			// Make sure that the module (for testing) is available, we count all file-path which end with the filename 
			var count = languageContainer.Files["en-US"].Count(file => file.EndsWith("language_mymodule-en-US.ini"));
			Assert.True(count > 0);
			Assert.Equal("Some value", loaderMyModuleTest.ModuleSettings);
		}

		[Fact]
		public async Task TestTranslations()
		{
			var languageLoaderTest = Language<ILanguageLoaderTest>.Create();
            using var languageContainer = await CreateContainer(languageLoaderTest);

			Assert.True(languageContainer.AvailableLanguages.ContainsKey("nl-NL"));
			Assert.True(languageContainer.AvailableLanguages.ContainsKey("en-US"));
			Assert.True(languageContainer.AvailableLanguages.ContainsKey("de-DE"));
			Assert.True(languageContainer.AvailableLanguages.ContainsKey("sr-Cyrl-RS"));
			Assert.Equal("Nederlands (Nederland)", languageContainer.AvailableLanguages["nl-NL"]);
			await languageContainer.ChangeLanguageAsync("en-US");
			Assert.Equal(Ok, languageLoaderTest.Ok);
			Assert.Equal("Cancel", languageLoaderTest.TestValue);
			Assert.Equal("BlubEN", languageLoaderTest.OnlyenUs);
			Assert.NotEqual("BlubNL", languageLoaderTest.OnlynlNl);
			Assert.NotEqual("BlubDE", languageLoaderTest.OnlydeDe);
			await languageContainer.ChangeLanguageAsync("nl-NL");
			Assert.Equal("Afbreken", languageLoaderTest.TestValue);
			Assert.NotEqual("BlubEN", languageLoaderTest.OnlyenUs);
			Assert.NotEqual("BlubDE", languageLoaderTest.OnlydeDe);
			Assert.Equal("BlubNL", languageLoaderTest.OnlynlNl);
			await languageContainer.ChangeLanguageAsync("de-DE");
			Assert.Equal("BlubDE", languageLoaderTest.OnlydeDe);
		}
	}
}