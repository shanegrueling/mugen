namespace Mugen.Benchmark.Benchmarks
{
    using System;
    using System.Runtime.InteropServices;
    using BenchmarkDotNet.Attributes;

    [MemoryDiagnoser]
    public class MarshalBenchmark
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Test
        {
            public int A;
        }

        private readonly IntPtr _pointer;
        private readonly unsafe Test* _testPointer;

        public unsafe MarshalBenchmark()
        {
            _pointer = Marshal.AllocHGlobal(sizeof(Test));
            _testPointer = (Test*) Marshal.AllocHGlobal(sizeof(Test));
        }

        private unsafe ref Test GetTest() => ref *(Test*) _pointer;
        private unsafe ref Test GetTest2() => ref * _testPointer;

        [Benchmark]
        public void MarshalToStruct()
        {
            ref var t = ref GetTest();

            t.A = 1806;

            ref var t2 = ref GetTest();

            if(t2.A != 1806) throw new Exception();
        }

        [Benchmark]
        public void MarshalToStruct2()
        {
            ref var t = ref GetTest2();

            t.A = 1806;

            ref var t2 = ref GetTest2();

            if(t2.A != 1806) throw new Exception();
        }
    }
}
