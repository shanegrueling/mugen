﻿using System;

namespace Sample.FoodHunter.NetCore
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Logic.Components;
    using Logic.Generated.ComponentSystems;
    using Mugen.Experimental;
    using Mugen.Math;

    internal static class Program
    {
        private static bool _shouldRun;

        private static async Task Main(string[] args)
        {
            using (var world = new World())
            {

                world.AddSystem(new SpawnFood(world));
                world.AddSystem(new SearchForFood(world));
                world.AddSystem(new ApplyVelocity(world));
                world.AddSystem(new EatFood(world));

                FillWorld(world);

                _shouldRun = true;
                Console.CancelKeyPress += Exit;

                var frames = 0;
                var s = Stopwatch.StartNew();
                while (_shouldRun)
                {
                    await world.Update(0.033f);
                    ++frames;
                }
                s.Stop();

                Console.WriteLine($"Frames: {frames}");
                Console.WriteLine($"Elapsed Time: {s.Elapsed}");
                Console.WriteLine($"Avg. Time per Frame: {frames/s.ElapsedMilliseconds}");
            }
            Console.ReadLine();
        }

        private static void FillWorld(World world)
        {
            var gameParameter = world.EntityManager.CreateEntity();
            world.EntityManager.AddComponent(gameParameter, new Size { Value = new int2(1000, 1000)});

            var eaterBlueprint = world.EntityManager.CreateBlueprint(
                typeof(Position),
                typeof(Velocity),
                typeof(Score));

            var r = new Random();

            for (var i = 0; i < 10; ++i)
            {
                var eater = world.EntityManager.CreateEntity(eaterBlueprint);
                world.EntityManager.ReplaceComponent(eater, new Position { Value = new float2(r.Next(0, 1000), r.Next(0, 1000))});
            }
        }

        private static void Exit(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("Exit!");
            _shouldRun = false;
        }
    }
}
