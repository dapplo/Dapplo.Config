using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class HasChangesImpl : ConfigurationBase<IHasChangesTest>, IHasChangesTest
    {
        #region Implementation of IHasChangesTest

        public string SayMyName { get; set; }

        #endregion
    }
}
