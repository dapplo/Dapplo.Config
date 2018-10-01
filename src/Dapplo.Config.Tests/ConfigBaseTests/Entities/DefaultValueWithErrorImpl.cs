using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class DefaultValueWithErrorImpl : ConfigurationBase<IDefaultValueWithErrorTest>, IDefaultValueWithErrorTest
    {
        #region Implementation of IDefaultValueWithErrorTest

        public SimpleEnum MyEnum { get; set; }

        #endregion
    }
}
