using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class TagAttributeImpl : DictionaryConfigurationBase<ITagAttributeTest>, ITagAttributeTest
    {
        public int Age { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }
    }
}
