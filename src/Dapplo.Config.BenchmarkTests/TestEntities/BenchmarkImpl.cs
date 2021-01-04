// Copyright (c) Dapplo and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Dapplo.Config.BenchmarkTests.TestInterfaces;

namespace Dapplo.Config.BenchmarkTests.TestEntities
{
    public class BenchmarkImpl : DictionaryConfiguration<IBenchmarkInterface>, IBenchmarkInterface
    {
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
