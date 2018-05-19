namespace Sample.FoodHunter.Logic.Generated.ComponentSystems
{
    using Components;
    using Mugen;
    using Mugen.Abstraction;
    using Mugen.Abstraction.Arrays;
    using Mugen.Abstraction.CommandBuffers;
    using Mugen.Abstraction.Systems;

    [GeneratedComponentSystem(typeof(Logic.ComponentSystems.EatFood))]
    public sealed class EatFood : AUpdateSystem
    {
        private readonly IEntityCommandBuffer<Logic.ComponentSystems.EatFood> _buffer;
        private readonly Eater _eater;
        private readonly Logic.ComponentSystems.EatFood _eatFood;
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

            public Eater(IComponentMatcher matcher)
            {
                _matcher = matcher;
                Positions = _matcher.GetComponentArray<Position>();
                Scores = _matcher.GetComponentArray<Score>();
            }

            public int Length => _matcher.Length;
            public IComponentArray<Position> Positions { get; }
            public IComponentArray<Score> Scores { get; }
        }

        private class Food : Logic.ComponentSystems.EatFood.IFood
        {
            private readonly IComponentMatcher _matcher;

            public Food(IComponentMatcher matcher)
            {
                _matcher = matcher;
                Positions = _matcher.GetComponentArray<Position>();
                PointValues = _matcher.GetComponentArray<PointValue>();
                Entities = _matcher.GetEntityArray();
            }

            public int Length => _matcher.Length;
            public IComponentArray<Position> Positions { get; }
            public IComponentArray<PointValue> PointValues { get; }
            public IEntityArray Entities { get; }
        }
    }
}