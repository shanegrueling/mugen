namespace Test.Mugen.Experimental.Components
{
    using global::Mugen.Abstraction;

    public struct Spawner : IComponent
    {
        public float SecondsBetweenSpawns;
        public float SecondsTillLastSpawn;
    }
}