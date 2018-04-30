namespace Mugen.Benchmark
{
    using BenchmarkDotNet.Running;
    using Benchmarks;

    class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MarshalBenchmark>();
        }
    }
}
