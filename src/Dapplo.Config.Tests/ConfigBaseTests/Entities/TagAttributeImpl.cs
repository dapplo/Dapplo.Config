﻿using Dapplo.Config.Tests.ConfigBaseTests.Interfaces;

namespace Dapplo.Config.Tests.ConfigBaseTests.Entities
{
    public class TagAttributeImpl : ConfigurationBase<ITagAttributeTest>, ITagAttributeTest
    {
        #region Implementation of ITagAttributeTest

        public int Age { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }

        #endregion
    }
}
