namespace Sample.FoodHunter.Logic.ComponentSystems
{
    using System;
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.CommandBuffers;
    using Mugen.Math;

    public class EatFood
    {
        private readonly IEntityCommandBuffer<EatFood> _buffer;

        public EatFood(IEntityManager manager)
        {
            _buffer = manager.CreateCommandBuffer<EatFood>();
        }

        public void Update(float deltaTime, IEater eater, IFood food)
        {
            for (var i = 0; i < food.Length; ++i)
            {
                ref var foodPosition = ref food.Positions[i];
                for (var j = 0; j < eater.Length; ++j)
                {
                    ref var eaterPosition = ref eater.Positions[j];

                    if(float2.Distance(foodPosition.Value, eaterPosition.Value) > 0.5f) continue;

                    ref var score = ref eater.Scores[j];
                    score.Value += food.PointValues[i].Value;

                    _buffer.DeleteEntity(food.Entities[i]);
                    break;
                }
            }
        }

        public interface IEater : IComponentGroup
        {
            IComponentArray<Position> Positions { get; }
            IComponentArray<Score> Scores { get; }
        }

        public interface IFood : IComponentGroup
        {
            IComponentArray<Position> Positions { get; }
            IComponentArray<PointValue> PointValues { get; }
            IEntityArray Entities { get; }
        }
    }
}
