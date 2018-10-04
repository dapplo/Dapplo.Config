using System;
using System.Collections.Generic;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class DefaultValueOverwriteImpl : DictionaryConfigurationBase<IDefaultValueOverwriteTest>, IDefaultValueOverwriteTest
    {
        public int Age { get; set; }
        public IList<int> Ages { get; set; }
        public IList<Uri> MyUris { get; set; }
    }
}
