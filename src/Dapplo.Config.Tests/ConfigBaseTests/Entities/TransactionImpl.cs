using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class TransactionImpl : ConfigurationBase<ITransactionTest>, ITransactionTest
    {
        #region Implementation of ITransactionTest

        public int Age { get; set; }

        #endregion
    }
}
