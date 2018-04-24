namespace Mugen.Benchmark.Generated.ComponentSystems
{
    using Components;

    internal class Move : IUpdateSystem
    {
        private readonly Mover _mover;
        public bool HasUpdateMethod => true;
        public bool IsAsync => false;

        public Move(World world)
        {
            _mover = new Mover(world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity)));
        }

        public void Update(float deltaTime)
        {
            Benchmark.ComponentSystems.Move.Update(deltaTime, _mover);
        }

        private class Mover : Benchmark.ComponentSystems.Move.IMover
        {
            private readonly IComponentMatcher _matcher;
            public int Length => _matcher.Length;
            public IComponentArray<Position> Positions => _matcher.GetComponentArray<Position>();
            public IComponentArray<Velocity> Velocities => _matcher.GetComponentArray<Velocity>();

            public Mover(IComponentMatcher matcher)
            {
                _matcher = matcher;
            }
        }
    }

    internal class Move2 : IUpdateSystem
    {
        private readonly IComponentMatcher _matcher;
        public bool HasUpdateMethod => true;
        public bool IsAsync => false;

        public Move2(World world)
        {
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity));
        }

        public void Update(float deltaTime)
        {
            var l = _matcher.Length;
            var p = _matcher.GetComponentArray<Position>();
            var v = _matcher.GetComponentArray<Velocity>();
            for(var i = 0; i < l; ++i)
            {
                Benchmark.ComponentSystems.Move.Update(deltaTime, ref p[i], ref v[i]);
            }
        }
    }

    internal class Move3 : IUpdateSystem
    {
        private readonly IComponentMatcher _matcher;
        public bool HasUpdateMethod => true;
        public bool IsAsync => false;

        public Move3(World world)
        {
            _matcher = world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity));
        }

        public void Update(float deltaTime)
        {
            Benchmark.ComponentSystems.Move.Update(deltaTime, _matcher.Length, _matcher.GetComponentArray<Position>(), _matcher.GetComponentArray<Velocity>());
        }
    }
}