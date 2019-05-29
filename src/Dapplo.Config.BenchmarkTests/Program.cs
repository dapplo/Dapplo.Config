using System;
using BenchmarkDotNet.Running;
using Dapplo.Log;

namespace Dapplo.Config.BenchmarkTests
{
    /// <summary>
    /// Performance tests for Dapplo.Config
    /// </summary>
    public static class Program
    {
        private static void Main(string[] args)
        {
            LogSettings.RegisterDefaultLogger<NullLogger>(LogLevels.Info);
            BenchmarkRunner.Run<BasicPerformance>();
            Console.ReadLine();
        }
    }
}
