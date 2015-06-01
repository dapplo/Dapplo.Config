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
	public class LanguageTest
	{
		public const string Ok = "Ok";

		[TestMethod]
		public async Task TestTranslations()
		{
			var languageLoader = new LanguageLoader("Dapplo");
			var language = await languageLoader.RegisterAndGetAsync<ILanguageTest>();
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("nl-NL"));
			Assert.IsTrue(languageLoader.AvailableLanguages.ContainsKey("en-US"));
			Assert.AreEqual("Nederlands (Nederland)", languageLoader.AvailableLanguages["nl-NL"]);

			Assert.AreEqual(Ok, language.Ok);
			Assert.AreEqual("Cancel", language.TestValue);
			Assert.AreEqual("Blub", language.OnlyenUS);
			Assert.AreNotEqual("Blub", language.OnlynlNL);
			await languageLoader.ChangeLanguage("nl-NL");
			Assert.AreEqual("Afbreken", language.TestValue);
			Assert.AreNotEqual("Blub", language.OnlyenUS);
			Assert.AreEqual("Blub", language.OnlynlNL);
		}
	}
}