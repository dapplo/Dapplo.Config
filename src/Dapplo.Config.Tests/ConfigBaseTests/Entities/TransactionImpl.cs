using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class TransactionImpl : DictionaryConfigurationBase<ITransactionTest>, ITransactionTest
    {
        public int Age { get; set; }
    }
}
