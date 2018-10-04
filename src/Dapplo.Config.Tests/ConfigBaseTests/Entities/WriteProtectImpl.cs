using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class WriteProtectImpl : DictionaryConfigurationBase<IWriteProtectTest>, IWriteProtectTest
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
