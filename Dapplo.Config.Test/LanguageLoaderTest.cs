using System.Threading.Tasks;
using Dapplo.Config.Language;
using Dapplo.Config.Test.TestInterfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dapplo.Config.Test
{
	/// <summary>
	/// This test class tests the transactional capabilities of the proxy
	/// </summary>
	[TestClass]
	public class LanguageLoaderTest
	{
		public const string Ok = "Ok";

		[TestMethod]
		public async Task TestTranslations()
		{
			var languageLoader = new LanguageLoader("Dapplo");
			var language = await languageLoader.RegisterAndGetAsync<ILanguageLoaderTest>();
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("nl-NL"));
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("en-US"));
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("de-DE"));
			Assert.AreEqual("Nederlands (Nederland)", languageLoader.AvailableLanguages["nl-NL"]);

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