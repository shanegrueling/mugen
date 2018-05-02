namespace Mugen.Benchmark.Benchmarks
{
    using System;
    using BenchmarkDotNet.Attributes;
    using Math;
    using Mugen.Components;
    using Mugen.Generated.ComponentSystems;

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

            for(var i = 0; i < Entites; ++i)
            {
                var mover = world.EntityManager.CreateEntity(blueprintMover);
                ref var pos = ref world.EntityManager.GetComponent<Position>(mover);
                pos.Value = new int2(r.Next(), r.Next());
                ref var vel = ref world.EntityManager.GetComponent<Velocity>(mover);
                vel.Value = new int2(r.Next(-5, 5), r.Next(-5, 5));

                var spawner = world.EntityManager.CreateEntity(blueprintSpawner);
                ref var sPos = ref world.EntityManager.GetComponent<Position>(spawner);
                sPos.Value = new int2(r.Next(), r.Next());
                ref var sSpawner = ref world.EntityManager.GetComponent<Spawner>(spawner);
                sSpawner.SecondsTillLastSpawn = (float)r.NextDouble();
                sSpawner.SecondsBetweenSpawns = 1;
            }

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
        public void UpdateWorldWithSpawners30Times()
        {
            for (var i = 0; i < 30; ++i)
            {
                _world4.Update(0.033f).Wait();
            }
        }

        [Benchmark]
        public void UpdateWorldWithSpawners300Times()
        {
            for (var i = 0; i < 300; ++i)
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
        public void WorldWithSpawnerAndMoverAndSimulating1SecondUpdates()
        {
            var world = new World(null);

            world.AddSystem(new Spawn(world));
            world.AddSystem(new Move2(world));
            var blueprintMover = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Velocity));
            var blueprintSpawner = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Spawner));

            var r = new Random();

            for (var i = 0; i < 5000; ++i)
            {
                var mover = world.EntityManager.CreateEntity(blueprintMover);
                ref var pos = ref world.EntityManager.GetComponent<Position>(mover);
                pos.Value = new int2(r.Next(), r.Next());
                ref var vel = ref world.EntityManager.GetComponent<Velocity>(mover);
                vel.Value = new int2(r.Next(-5, 5), r.Next(-5, 5));

                var spawner = world.EntityManager.CreateEntity(blueprintSpawner);
                ref var sPos = ref world.EntityManager.GetComponent<Position>(spawner);
                sPos.Value = new int2(r.Next(), r.Next());
                ref var sSpawner = ref world.EntityManager.GetComponent<Spawner>(spawner);
                sSpawner.SecondsTillLastSpawn = (float)r.NextDouble();
                sSpawner.SecondsBetweenSpawns = 1;
            }

            for (var i = 0; i < 30; ++i)
            {
                world.Update(0.033f).Wait();
            }
        }
    }
}
