namespace Sample.FoodHunter.Logic.Generated.ComponentSystems
{
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.Systems;
    using Mugen.Experimental;

    [GeneratedComponentSystem(typeof(Logic.ComponentSystems.ApplyVelocity))]
    public sealed class ApplyVelocity : AUpdateSystem
    {
        private readonly IComponentMatcher _matcher;

        public ApplyVelocity(World world)
        {
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity));
        }

        public override void Update(float deltaTime)
        {
            var l = _matcher.Length;
            var p = _matcher.GetComponentArray<Position>();
            var v = _matcher.GetComponentArray<Velocity>();

            for (var i = 0; i < l; ++i)
            {
                Logic.ComponentSystems.ApplyVelocity.Update(deltaTime, ref p[i], ref v[i]);
            }
        }
    }
}
