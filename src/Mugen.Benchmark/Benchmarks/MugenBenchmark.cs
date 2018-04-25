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

        private readonly World _world4;

        private const int Entites = 5000;

        public MugenBenchmark()
        {
            _world = CreateWorld<Move>();
            _world2 = CreateWorld<Move2>();
            _world3 = CreateWorld<Move3>();

            _world4 = CreateWorld<Spawn>();
        }

        private static World CreateWorld<T>() where T : ISystem
        {
            var world = new World(null);

            world.AddSystem((T)Activator.CreateInstance(typeof(T), world));
            var blueprintMover = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Velocity));
            var blueprintSpawner = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Spawner));

            var r = new Random();

            var buffer = world.EntityManager.CreateBuffer();

            for(var i = 0; i < Entites; ++i)
            {
                buffer.CreateEntity(blueprintMover)
                      .ReplaceComponent(new Position {Value = new int2(r.Next(), r.Next())})
                      .ReplaceComponent(new Velocity {Value = new int2(r.Next(-5, 5), r.Next(-5, 5))})
                      .Finish();

                buffer.CreateEntity(blueprintSpawner)
                    .ReplaceComponent(new Position {Value = new int2(r.Next(), r.Next())})
                    .ReplaceComponent(new Spawner { SecondsTillLastSpawn = (float)r.NextDouble(), SecondsBetweenSpawns = 1})
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

        [Benchmark]
        public void UpdateWorldWithSpawners10Times()
        {
            for (var i = 0; i < 10; ++i)
            {
                _world4.Update(0.033f).Wait();
            }
        }

        [Benchmark]
        public void CreateWorld()
        {
            var world = CreateWorld<Spawn>();
        }

        [Benchmark]
        public void WorldWithSpawnerAndMover()
        {
            var world = new World(null);

            world.AddSystem(new Spawn(world));
            world.AddSystem(new Move2(world));
            var blueprintMover = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Velocity));
            var blueprintSpawner = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Spawner));

            var r = new Random();

            var buffer = world.EntityManager.CreateBuffer();

            for (var i = 0; i < 5000; ++i)
            {
                buffer.CreateEntity(blueprintMover)
                    .ReplaceComponent(new Position {Value = new int2(r.Next(), r.Next())})
                    .ReplaceComponent(new Velocity {Value = new int2(r.Next(-5, 5), r.Next(-5, 5))}).Finish();

                buffer.CreateEntity(blueprintSpawner)
                    .ReplaceComponent(new Position {Value = new int2(r.Next(), r.Next())})
                    .ReplaceComponent(new Spawner {SecondsTillLastSpawn = (float) r.NextDouble(), SecondsBetweenSpawns = 1})
                    .Finish();
            }

            buffer.Playback();

            for (var i = 0; i < 30; ++i)
            {
                world.Update(0.033f).Wait();
            }
        }
    }
}
