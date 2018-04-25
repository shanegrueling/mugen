namespace Mugen.Benchmark.ComponentSystems
{
    using Components;
    using Math;

    internal class Spawn
    {
        private readonly Blueprint _blueprint;
        public IEntityCommandBuffer Buffer { get; set; }

        public Spawn(IEntityManager manager)
        {
            _blueprint = manager.CreateBlueprint(typeof(Position), typeof(Velocity));
        }

        public void Update(float deltaTime, int length, IComponentArray<Position> positions, IComponentArray<Spawner> spawners)
        {
            for (var i = 0; i < length; i++)
            {
                ref var spawner = ref spawners[i];

                spawner.SecondsTillLastSpawn += deltaTime;

                if (!(spawner.SecondsTillLastSpawn >= spawner.SecondsBetweenSpawns))
                {
                    continue;
                }

                spawner.SecondsTillLastSpawn -= spawner.SecondsBetweenSpawns;
                
                Buffer.CreateEntity(_blueprint)
                    .ReplaceComponent(positions[i])
                    .ReplaceComponent(new Velocity {Value = new int2(1, 1)});
            }
        }
    }
}
