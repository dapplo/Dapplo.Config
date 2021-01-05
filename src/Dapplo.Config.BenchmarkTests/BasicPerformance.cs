// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using BenchmarkDotNet.Attributes;
using Dapplo.Config.BenchmarkTests.TestEntities;
using Dapplo.Config.BenchmarkTests.TestInterfaces;

namespace Dapplo.Config.BenchmarkTests
{
    [MinColumn, MaxColumn, MemoryDiagnoser]
    public class BasicPerformance
    {
        [Benchmark]
        public void BasicConfig()
        {
            IBenchmarkInterface benchmark = new BenchmarkImpl();
            benchmark.Age = 10;
            benchmark.Name = "Dapplo";
        }
    }
}
