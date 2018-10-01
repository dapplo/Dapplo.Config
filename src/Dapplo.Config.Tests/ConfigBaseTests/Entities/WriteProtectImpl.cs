using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class WriteProtectImpl : ConfigurationBase<IWriteProtectTest>, IWriteProtectTest
    {
        #region Implementation of IWriteProtectTest

        public int Age { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
