using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class CloneableImpl : DictionaryConfigurationBase<ICloneableTest>, ICloneableTest
    {
        #region Implementation of ICloneableTest

        public string Name { get; set; }

        #endregion
    }
}
