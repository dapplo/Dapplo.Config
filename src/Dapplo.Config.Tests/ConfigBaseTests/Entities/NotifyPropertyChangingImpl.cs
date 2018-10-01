using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class NotifyPropertyChangingImpl : ConfigurationBase<INotifyPropertyChangingTest>, INotifyPropertyChangingTest
    {
        #region Implementation of INotifyPropertyChangingTest

        public string Name { get; set; }

        #endregion
    }
}
