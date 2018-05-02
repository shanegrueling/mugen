namespace Mugen.Benchmark.Benchmarks
{
    using BenchmarkDotNet.Attributes;
    using Mugen;

    //[DisassemblyDiagnoser(printAsm: true, printSource: true)]
    [MemoryDiagnoser]
    //[KeepBenchmarkFiles]
    public class MugenVsExperimentelBenchmark
    {
        [Params(1000, 5000)]
        public int Entites { get; set; }

        [Params(0, 10, 100)]
        public int Frames { get; set; }

        [Benchmark]
        public void Mugen()
        {
            WorldTest.MoverBenchmark(Entites, Frames);
        }

        [Benchmark]
        public void Experimental()
        {
            Benchmark.Mugen.Experimental.WorldTest.MoverBenchmark(Entites, Frames);
        }
    }
}