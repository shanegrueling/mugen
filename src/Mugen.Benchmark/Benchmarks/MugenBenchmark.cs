namespace Mugen.Benchmark.Benchmarks
{
    using System;
    using BenchmarkDotNet.Attributes;
    using Components;
    using Generated.ComponentSystems;
    using Math;

    [MemoryDiagnoser]
    public class MugenBenchmark
    {
        private readonly World _world;

        private readonly World _world2;

        private readonly World _world3;

        private const int Entites = 5000;

        public MugenBenchmark()
        {
            _world = CreateWorld<Move>();
            _world2 = CreateWorld<Move2>();
            _world3 = CreateWorld<Move3>();
        }

        private static World CreateWorld<T>() where T : ISystem
        {
            var world = new World(null);

            world.AddSystem((T)Activator.CreateInstance(typeof(T), world));
            var blueprint = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Velocity));

            var r = new Random();

            var buffer = world.EntityManager.CreateBuffer();

            for(var i = 0; i < Entites; ++i)
            {
                buffer.CreateEntity(blueprint)
                      .ReplaceComponent(new Position {Value = new int2(r.Next(), r.Next())})
                      .ReplaceComponent(new Velocity {Value = new int2(r.Next(-5, 5), r.Next(-5, 5))})
                      .Finish();
            }

            buffer.Playback();

            return world;
        }

        [Benchmark]
        public void UpdateWorldWithIComponentGroup()
        {
            _world.Update(0.2f).Wait();
        }

        [Benchmark]
        public void UpdateWorldWithDirectComponents()
        {
            _world2.Update(0.2f).Wait();
        }

        [Benchmark]
        public void UpdateWorldWithComponentArrays()
        {
            _world3.Update(0.2f).Wait();
        }
    }
}
