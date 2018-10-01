using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class DescriptionImpl : ConfigurationBase<IDescriptionTest>, IDescriptionTest
    {
        #region Implementation of IDescriptionTest

        public string Name { get; set; }

        #endregion
    }
}
