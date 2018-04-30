namespace Sample.FoodHunter.Logic.ComponentSystems
{
    using System;
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.CommandBuffers;
    using Mugen.Math;

    public class SpawnFood
    {
        private readonly IEntityCommandBuffer<SpawnFood> _buffer;
        private readonly Blueprint _foodBlueprint;

        private readonly Random _random;

        public SpawnFood(IEntityManager manager)
        {
            _buffer = manager.CreateCommandBuffer<SpawnFood>();
            _foodBlueprint = manager.CreateBlueprint(typeof(Position), typeof(PointValue));
            _random = new Random();
        }

        public void Update(float deltaTime, IFood food, IGameParameter gameParameter)
        {
            if (food.Length > 0) return;
            
            var size = gameParameter.Sizes[0];

            _buffer.CreateEntity(_foodBlueprint)
                .SetComponent(new Position {Value = new float2(_random.Next(0, size.Value.X), _random.Next(0, size.Value.Y))})
                .SetComponent(new PointValue { Value = 10 })
                .Finish();
        }

        public interface IFood : IComponentGroup
        {
            IComponentArray<Position> Positions { get; }
            IComponentArray<PointValue> PointValues { get; }
        }

        public interface IGameParameter : IComponentGroup
        {
            IComponentArray<Size> Sizes { get; }
        }
    }
}
