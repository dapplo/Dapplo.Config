using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class DescriptionImpl : DictionaryConfigurationBase<IDescriptionTest>, IDescriptionTest
    {
        public string Name { get; set; }
    }
}
