namespace Mugen.Benchmark.Benchmarks
{
    using BenchmarkDotNet.Attributes;

    public class InterfaceDispatch
    {
        private ITest1 _test1;
        private ITest2 _test2;

        private Test1 _test1D;

        public interface ITest1
        {
            int Int { get; }
        }

        public class Test1 : ITest1
        {
            public int Int { get; set; }
        }

        public interface ITest2
        {
            int Int { get; }
        }

        public sealed class Test2 : ITest2
        {
            public int Int { get; set; }
        }

        public InterfaceDispatch()
        {
            _test1 = new Test1() { Int = 1806};
            _test1D = new Test1() {Int = 1906};
            _test2 = new Test2() { Int = 1806};
        }
        
        [Benchmark]
        public int NoSealed()
        {
            return 12 + _test1.Int;
        }

        [Benchmark]
        public int Sealed()
        {
            return 12 + _test2.Int;
        }

        [Benchmark]
        public int Direct()
        {
            return 12 + _test1D.Int;
        }
    }
}
