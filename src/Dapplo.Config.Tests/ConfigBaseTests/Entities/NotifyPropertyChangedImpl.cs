using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class NotifyPropertyChangedImpl : ConfigurationBase<INotifyPropertyChangedTest>, INotifyPropertyChangedTest
    {
        #region Implementation of INotifyPropertyChangedTest

        public string Name { get; set; }

        #endregion
    }
}
