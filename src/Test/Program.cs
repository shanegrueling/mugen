﻿using System;

namespace Test
{
    using global::Mugen.Experimental;
    using global::Mugen.Math;
    using Mugen.Experimental.Components;
    using Mugen.Experimental.Generated.ComponentSystems;

    class Program
    {
        static void Main(string[] args)
        {
            var world = new World();

            world.AddSystem(new Move2(world));
            var blueprintMover = world.EntityManager.CreateBlueprint(typeof(Position), typeof(Velocity));

            var r = new Random();

            for (var i = 0; i < 1000; ++i)
            {
                var mover = world.EntityManager.CreateEntity(blueprintMover);
                ref var pos = ref world.EntityManager.GetComponent<Position>(mover);
                pos.Value = new int2(r.Next(), r.Next());
                ref var vel = ref world.EntityManager.GetComponent<Velocity>(mover);
                vel.Value = new int2(r.Next(-5, 5), r.Next(-5, 5));
            }

            for (var i = 0; i < 300; ++i)
            {
                world.Update(0.033f).Wait();
            }

            Console.ReadLine();
        }
    }
}
