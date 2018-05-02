namespace Mugen.Benchmark.Mugen.Experimental.Generated.ComponentSystems
{
    using Abstraction;
    using Abstraction.Systems;
    using Components;
    using World = global::Mugen.Experimental.World;

    internal class Move : AUpdateSystem
    {
        private readonly Mover _mover;

        public Move(World world)
        {
            _mover = new Mover(world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity)));
        }

        public bool HasUpdateMethod => true;

        public override void Update(float deltaTime)
        {
            Experimental.ComponentSystems.Move.Update(_mover);
        }

        private class Mover : Experimental.ComponentSystems.Move.IMover
        {
            private readonly IComponentMatcher _matcher;

            public Mover(IComponentMatcher matcher)
            {
                _matcher = matcher;
            }

            public IComponentArray<Position> Positions => _matcher.GetComponentArray<Position>();
            public IComponentArray<Velocity> Velocities => _matcher.GetComponentArray<Velocity>();
            public int Length => _matcher.Length;
        }
    }

    public class Move2 : AUpdateSystem
    {
        private readonly IComponentMatcher _matcher;

        public Move2(World world)
        {
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity));
        }

        public bool HasUpdateMethod => true;

        public override void Update(float deltaTime)
        {
            var l = _matcher.Length;
            var p = _matcher.GetComponentArray<Position>();
            var v = _matcher.GetComponentArray<Velocity>();
            for (var i = 0; i < l; ++i)
            {
                Experimental.ComponentSystems.Move.Update(ref p[i], v[i]);
            }
        }
    }

    internal class Move3 : AUpdateSystem
    {
        private readonly IComponentMatcher _matcher;

        public Move3(World world)
        {
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity));
        }

        public bool HasUpdateMethod => true;

        public override void Update(float deltaTime)
        {
            Experimental.ComponentSystems.Move.Update(
                _matcher.Length,
                _matcher.GetComponentArray<Position>(),
                _matcher.GetComponentArray<Velocity>());
        }
    }
}