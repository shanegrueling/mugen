namespace Sample.FoodHunter.Logic.Generated.ComponentSystems
{
    using Components;
    using Mugen;
    using Mugen.Abstraction;
    using Mugen.Abstraction.Arrays;
    using Mugen.Abstraction.Systems;

    [GeneratedComponentSystem(typeof(Logic.ComponentSystems.ApplyVelocity))]
    public sealed class ApplyVelocity : AUpdateSystem
    {
        private readonly IComponentMatcher _matcher;
        private readonly IComponentArray<Position> _p;
        private readonly IComponentArray<Velocity> _v;

        public ApplyVelocity(World world)
        {
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity));
            _p = _matcher.GetComponentArray<Position>();
            _v = _matcher.GetComponentArray<Velocity>();
        }

        public override void Update(float deltaTime)
        {
            var l = _matcher.Length;

            for (var i = 0; i < l; ++i)
            {
                Logic.ComponentSystems.ApplyVelocity.Update(deltaTime, ref _p[i], ref _v[i]);
            }
        }
    }
}