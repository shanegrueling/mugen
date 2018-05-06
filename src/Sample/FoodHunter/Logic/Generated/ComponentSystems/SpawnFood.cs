namespace Sample.FoodHunter.Logic.Generated.ComponentSystems
{
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.CommandBuffers;
    using Mugen.Abstraction.Systems;
    using Mugen.Experimental;

    [GeneratedComponentSystem(typeof(Logic.ComponentSystems.SpawnFood))]
    public sealed class SpawnFood : AUpdateSystem
    {
        private readonly Logic.ComponentSystems.SpawnFood _system;
        private readonly IEntityCommandBuffer<Logic.ComponentSystems.SpawnFood> _buffer;
        private readonly Food _food;
        private readonly GameParameter _gameParameter;

        public SpawnFood(World world)
        {
            _system = new Logic.ComponentSystems.SpawnFood(world.EntityManager);
            _buffer = world.EntityManager.GetCommandBuffer<Logic.ComponentSystems.SpawnFood>();

            _food = new Food(world.EntityManager.GetMatcher(typeof(Position), typeof(PointValue)));
            _gameParameter = new GameParameter(world.EntityManager.GetMatcher(typeof(Size)));
        }

        public override void Update(float deltaTime)
        {
            _system.Update(deltaTime, _food, _gameParameter);
            _buffer?.Playback();
        }

        private sealed class Food : Logic.ComponentSystems.SpawnFood.IFood
        {
            private readonly IComponentMatcher _matcher;
            
            public int Length => _matcher.Length;
            public IComponentArray<Position> Positions { get; }
            public IComponentArray<PointValue> PointValues { get; }

            public Food(IComponentMatcher matcher)
            {
                _matcher = matcher;

                Positions = _matcher.GetComponentArray<Position>();
                PointValues = _matcher.GetComponentArray<PointValue>();
            }
        }

        private sealed class GameParameter : Logic.ComponentSystems.SpawnFood.IGameParameter
        {
            private readonly IComponentMatcher _matcher;
            
            public int Length => _matcher.Length;
            public IComponentArray<Size> Sizes { get; }

            public GameParameter(IComponentMatcher matcher)
            {
                _matcher = matcher;

                Sizes = _matcher.GetComponentArray<Size>();
            }
        }
    }
}