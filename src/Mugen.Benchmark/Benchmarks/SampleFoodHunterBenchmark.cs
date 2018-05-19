namespace Mugen.Benchmark.Benchmarks
{
    using System;
    using System.Threading.Tasks;
    using BenchmarkDotNet.Attributes;
    using Math;
    using Sample.FoodHunter.Logic.Components;
    using Sample.FoodHunter.Logic.Generated.ComponentSystems;
    
    [MemoryDiagnoser]
    public class SampleFoodHunterBenchmark
    {
        [Params(0, 10, 100, 1000)]
        public int Frames { get;set; }

        [Benchmark]
        public void CreateAndRunWorld()
        {
            using (var world = new World())
            {

                world.AddSystem(new SpawnFood(world));
                world.AddSystem(new SearchForFood(world));
                world.AddSystem(new ApplyVelocity(world));
                world.AddSystem(new EatFood(world));

                FillWorld(world);

                
                for(var i = 0; i < Frames; ++i)
                {
                    world.Update(0.033f);
                }
            }
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
    }
}
