using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class NotifyPropertyChangingImpl : DictionaryConfigurationBase<INotifyPropertyChangingTest>, INotifyPropertyChangingTest
    {
        public string Name { get; set; }
    }
}
