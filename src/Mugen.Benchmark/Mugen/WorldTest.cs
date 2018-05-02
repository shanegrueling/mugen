namespace Mugen.Benchmark.Mugen
{
    using System;
    using Components;
    using Generated.ComponentSystems;
    using Math;

    public static class WorldTest
    {
        public static void MoverBenchmark(int entites, int frames)
        {
            var world = new World(null);

            world.AddSystem(new Move2(world));
            var blueprintMover = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Velocity));

            var r = new Random();

            for (var i = 0; i < entites; ++i)
            {
                var mover = world.EntityManager.CreateEntity(blueprintMover);
                ref var pos = ref world.EntityManager.GetComponent<Position>(mover);
                pos.Value = new int2(r.Next(), r.Next());
                ref var vel = ref world.EntityManager.GetComponent<Velocity>(mover);
                vel.Value = new int2(r.Next(-5, 5), r.Next(-5, 5));
            }

            for (var i = 0; i < frames; ++i)
            {
                var t = world.Update(0.033f);
                if (!t.IsCompleted)
                {
                    t.Wait();
                }
            }
        }
    }
}