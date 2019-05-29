using BenchmarkDotNet.Attributes;
using Dapplo.Config.BenchmarkTests.TestEntities;
using Dapplo.Config.BenchmarkTests.TestInterfaces;

namespace Dapplo.Config.BenchmarkTests
{
    [MinColumn, MaxColumn, MemoryDiagnoser]
    public class BasicPerformance
    {
        private readonly IBenchmarkInterface _benchmark = new BenchmarkImpl();

        [Benchmark]
        public void BasicConfig()
        {
            _benchmark.Age = 10;
            _benchmark.Name = "Dapplo";
        }
    }
}
