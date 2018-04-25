namespace Mugen.Benchmark.Generated.ComponentSystems
{
    using Components;

    public class Spawn : IUpdateSystem
    {
        private readonly Benchmark.ComponentSystems.Spawn _system;
        private readonly IComponentMatcher _matcher;

        public bool HasUpdateMethod => true;
        public bool IsAsync => false;

        public Spawn(World world)
        {
            _system = new Benchmark.ComponentSystems.Spawn(world.EntityManager)
            {
                Buffer = world.EntityManager.CreateBuffer()
            };
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Spawner));
        }

        public void Update(float deltaTime)
        {
            _system.Update(deltaTime, _matcher.Length, _matcher.GetComponentArray<Position>(), _matcher.GetComponentArray<Spawner>());
            _system.Buffer.Playback();
        }
    }
}
