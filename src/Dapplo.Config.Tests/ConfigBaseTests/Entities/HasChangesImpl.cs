using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class HasChangesImpl : DictionaryConfigurationBase<IHasChangesTest>, IHasChangesTest
    {
        public HasChangesImpl()
        {
            TrackChanges();
        }
        public string SayMyName { get; set; }
    }
}
