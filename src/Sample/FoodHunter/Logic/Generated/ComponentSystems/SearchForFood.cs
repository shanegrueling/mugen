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

            public IComponentArray<Position> Positions => _matcher.GetComponentArray<Position>();
            public IComponentArray<Velocity> Velocity => _matcher.GetComponentArray<Velocity>();
            public IComponentArray<Score> Score => _matcher.GetComponentArray<Score>();

            public Eater(IComponentMatcher matcher)
            {
                _matcher = matcher;
            }
        }

        private sealed class Food : Logic.ComponentSystems.SearchForFood.IFood
        {
            private readonly IComponentMatcher _matcher;

            public int Length => _matcher.Length;

            public IComponentArray<Position> Positions => _matcher.GetComponentArray<Position>();
            public IComponentArray<PointValue> PointValue => _matcher.GetComponentArray<PointValue>();

            public Food(IComponentMatcher matcher)
            {
                _matcher = matcher;
            }
        }
    }
}