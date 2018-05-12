namespace Mugen.Benchmark.Mugen.Experimental
{
    using System;
    using Generated.ComponentSystems;
    using Math;
    using World = global::Mugen.Experimental.World;

    public static class WorldTest
    {
        public static void MoverBenchmark(int entities, int frames)
        {
            using (var world = new World())
            {

                world.AddSystem(new Move2(world));
                var blueprintMover = world.EntityManager.CreateBlueprint(
                    typeof(Components.Position),
                    typeof(Components.Velocity));

                var r = new Random();

                for (var i = 0; i < entities; ++i)
                {
                    var mover = world.EntityManager.CreateEntity(blueprintMover);
                    ref var pos = ref world.EntityManager.GetComponent<Components.Position>(mover);
                    pos.Value = new int2(r.Next(), r.Next());
                    ref var vel = ref world.EntityManager.GetComponent<Components.Velocity>(mover);
                    vel.Value = new int2(r.Next(-5, 5), r.Next(-5, 5));
                }

                for (var i = 0; i < frames; ++i)
                {
                    world.Update(0.033f);
                }
            }
        }

        public static void MoverBenchmark2(int entities, int frames)
        {
            using (var world = new World())
            {

                world.AddSystem(new Move22(world));
                var blueprintMover = world.EntityManager.CreateBlueprint(
                    typeof(Components.Position),
                    typeof(Components.Velocity));

                var r = new Random();

                for (var i = 0; i < entities; ++i)
                {
                    var mover = world.EntityManager.CreateEntity(blueprintMover);
                    ref var pos = ref world.EntityManager.GetComponent<Components.Position>(mover);
                    pos.Value = new int2(r.Next(), r.Next());
                    ref var vel = ref world.EntityManager.GetComponent<Components.Velocity>(mover);
                    vel.Value = new int2(r.Next(-5, 5), r.Next(-5, 5));
                }

                for (var i = 0; i < frames; ++i)
                {
                    world.Update(0.033f);
                }
            }
        }
    }
}