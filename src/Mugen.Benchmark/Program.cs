namespace Mugen.Benchmark
{
    using System.Linq;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Running;
    using BenchmarkDotNet.Toolchains.InProcess;
    using BenchmarkDotNet.Validators;
    using Benchmarks;

    class Program
    {
        public class DebugConfig : ManualConfig
        {
            public DebugConfig()
            {
                Add(JitOptimizationsValidator.DontFailOnError); // ALLOW NON-OPTIMIZED DLLS

                Add(Job.ShortRun
                    .WithLaunchCount(1)
                    .With(InProcessToolchain.Instance)); // use InProcessToolchain to be able to Debug in-process

                Add(DefaultConfig.Instance.GetLoggers().ToArray()); // manual config has no loggers by default
                Add(DefaultConfig.Instance.GetExporters().ToArray()); // manual config has no exporters by default
                Add(DefaultConfig.Instance.GetColumnProviders().ToArray()); // manual config has no columns by default
            }
        }

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<SampleFoodHunterBenchmark>();
        }
    }
}
