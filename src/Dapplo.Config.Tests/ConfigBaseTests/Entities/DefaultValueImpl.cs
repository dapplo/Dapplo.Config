using System;
using System.Collections.Generic;
using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class DefaultValueImpl : DictionaryConfigurationBase<IDefaultValueTest>, IDefaultValueTest
    {
        public int Age { get; set; }
        public IList<int> Ages { get; set; }
        public IList<Uri> MyUris { get; set; }
    }
}
