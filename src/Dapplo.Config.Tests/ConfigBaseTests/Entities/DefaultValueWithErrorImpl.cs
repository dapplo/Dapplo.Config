using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class DefaultValueWithErrorImpl : DictionaryConfigurationBase<IDefaultValueWithErrorTest>, IDefaultValueWithErrorTest
    {
        public SimpleEnum MyEnum { get; set; }
    }
}
