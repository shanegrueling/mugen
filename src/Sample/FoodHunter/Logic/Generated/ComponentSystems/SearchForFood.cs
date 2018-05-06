namespace Sample.FoodHunter.Logic.Generated.ComponentSystems
{
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.Systems;
    using Mugen.Experimental;

    [GeneratedComponentSystem(typeof(Logic.ComponentSystems.SearchForFood))]
    public sealed class SearchForFood : AUpdateSystem
    {
        private readonly Eater _eater;
        private readonly Food _food;

        public SearchForFood(World world)
        {
            _eater = new Eater(world.EntityManager.GetMatcher(typeof(Position), typeof(Velocity), typeof(Score)));
            _food = new Food(world.EntityManager.GetMatcher(typeof(Position), typeof(PointValue)));
        }

        public override void Update(float deltaTime)
        {
            Logic.ComponentSystems.SearchForFood.Update(deltaTime, _eater, _food);
        }

        private sealed class Eater : Logic.ComponentSystems.SearchForFood.IEater
        {
            private readonly IComponentMatcher _matcher;

            public int Length => _matcher.Length;

            public IComponentArray<Position> Positions { get; }
            public IComponentArray<Velocity> Velocity { get; }
            public IComponentArray<Score> Score { get; }

            public Eater(IComponentMatcher matcher)
            {
                _matcher = matcher;
                Positions = _matcher.GetComponentArray<Position>();
                Velocity = _matcher.GetComponentArray<Velocity>();
                Score = _matcher.GetComponentArray<Score>();
            }
        }

        private sealed class Food : Logic.ComponentSystems.SearchForFood.IFood
        {
            private readonly IComponentMatcher _matcher;

            public int Length => _matcher.Length;

            public IComponentArray<Position> Positions { get; }
            public IComponentArray<PointValue> PointValue { get; }

            public Food(IComponentMatcher matcher)
            {
                _matcher = matcher;

                Positions = _matcher.GetComponentArray<Position>();
                PointValue = _matcher.GetComponentArray<PointValue>();
            }
        }
    }
}