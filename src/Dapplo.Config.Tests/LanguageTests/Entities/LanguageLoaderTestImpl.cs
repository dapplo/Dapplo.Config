
using Dapplo.Config.Language;
using Dapplo.Config.Tests.LanguageTests.Interfaces;

namespace Dapplo.Config.Tests.LanguageTests.Entities
{
    public class LanguageLoaderTestImpl : LanguageBase<ILanguageLoaderTest>, ILanguageLoaderTest
    {
        #region Implementation of ILanguageLoaderPartTest

        public string Ok2 { get; }

        #endregion

        #region Implementation of ILanguageLoaderTest

        public string Ok { get; }
        public string OnlydeDe { get; }
        public string OnlyenUs { get; }
        public string OnlynlNl { get; }
        public string TestValue { get; }

        #endregion
    }
}
