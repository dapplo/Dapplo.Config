using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class IndexerImpl : DictionaryConfigurationBase<IIndexerTest>, IIndexerTest
    {
        public string Name { get; set; }
    }
}
