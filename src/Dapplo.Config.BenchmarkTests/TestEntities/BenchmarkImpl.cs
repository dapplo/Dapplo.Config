using Dapplo.Config.BenchmarkTests.TestInterfaces;

namespace Dapplo.Config.BenchmarkTests.TestEntities
{
    public class BenchmarkImpl : DictionaryConfiguration<IBenchmarkInterface>, IBenchmarkInterface
    {
        #region Implementation of IBenchmarkInterface

        public int Age { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
