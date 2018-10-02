using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dapplo.Config.Tests.ConfigTests.Interfaces;
using Dapplo.Registry;

namespace Dapplo.Config.Tests.ConfigTests.Entities
{
    public class RegistryTestImpl : RegistryBase<IRegistryTest>, IRegistryTest
    {
        public Dictionary<string, object> CuRun32 { get; set; }
        public Dictionary<string, object> CuRun64 { get; set; }
        public Dictionary<string, object> LmRun32 { get; set; }
        public Dictionary<string, object> LmRun64 { get; set; }
        public string ProductName { get; set; }
    }
}
