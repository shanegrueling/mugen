namespace Mugen.Benchmark.Mugen.Experimental.Components
{
    using Abstraction;

    public struct Spawner : IComponent
    {
        public float SecondsBetweenSpawns;
        public float SecondsTillLastSpawn;
    }
}