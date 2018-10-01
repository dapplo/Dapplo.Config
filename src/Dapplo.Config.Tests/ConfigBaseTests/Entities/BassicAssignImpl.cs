using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class BassicAssignImpl : ConfigurationBase<BassicAssignImpl>, IBassicAssignTest
    {
        public string Name { get; set; }
    }
}
