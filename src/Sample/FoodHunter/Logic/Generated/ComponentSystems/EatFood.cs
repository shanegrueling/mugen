namespace Sample.FoodHunter.Logic.Generated.ComponentSystems
{
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.CommandBuffers;
    using Mugen.Abstraction.Systems;
    using Mugen.Experimental;

    [GeneratedComponentSystem(typeof(Logic.ComponentSystems.EatFood))]
    public sealed class EatFood : AUpdateSystem
    {
        private readonly Logic.ComponentSystems.EatFood _eatFood;
        private readonly IEntityCommandBuffer<Logic.ComponentSystems.EatFood> _buffer;
        private readonly Eater _eater;
        private readonly Food _food;

        public EatFood(World world)
        {
            _eatFood = new Logic.ComponentSystems.EatFood(world.EntityManager);
            _buffer = world.EntityManager.GetCommandBuffer<Logic.ComponentSystems.EatFood>();

            _eater = new Eater(world.EntityManager.GetMatcher(typeof(Position), typeof(Score)));
            _food = new Food(world.EntityManager.GetMatcher(typeof(Position), typeof(PointValue)));
        }

        public override void Update(float deltaTime)
        {
            _eatFood.Update(deltaTime, _eater, _food);
            _buffer?.Playback();
        }

        private class Eater : Logic.ComponentSystems.EatFood.IEater
        {
            private readonly IComponentMatcher _matcher;

            public int Length => _matcher.Length;
            public IComponentArray<Position> Positions => _matcher.GetComponentArray<Position>();
            public IComponentArray<Score> Scores => _matcher.GetComponentArray<Score>();

            public Eater(IComponentMatcher matcher)
            {
                _matcher = matcher;
            }
        }

        private class Food : Logic.ComponentSystems.EatFood.IFood
        {
            private readonly IComponentMatcher _matcher;

            public int Length => _matcher.Length;
            public IComponentArray<Position> Positions => _matcher.GetComponentArray<Position>();
            public IComponentArray<PointValue> PointValues => _matcher.GetComponentArray<PointValue>();
            public IEntityArray Entities => _matcher.GetEntityArray();

            public Food(IComponentMatcher matcher)
            {
                _matcher = matcher;
            }
        }
    }
}