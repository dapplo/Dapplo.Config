using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class NotifyPropertyChangedImpl : DictionaryConfigurationBase<INotifyPropertyChangedTest>, INotifyPropertyChangedTest
    {
        public string Name { get; set; }
    }
}
