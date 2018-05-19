namespace Sample.FoodHunter.Logic.ComponentSystems
{
    using Components;
    using Mugen.Abstraction;
    using Mugen.Abstraction.Arrays;
    using Mugen.Math;

    public static class SearchForFood
    {
        public static void Update(float deltaTime, IEater eater, IFood food)
        {
            for (var i = 0; i < eater.Length; ++i)
            {
                ref var eaterPosition = ref eater.Positions[i];
                ref var eaterVelocity = ref eater.Velocity[i];

                var closestDistance = float.MaxValue;
                for (var j = 0; j < food.Length; ++j)
                {
                    ref var foodPosition = ref food.Positions[j];
                    var distance = float2.Distance(eaterPosition.Value, foodPosition.Value);
                    if (closestDistance <= distance)
                    {
                        return;
                    }

                    closestDistance = distance;

                    eaterVelocity.Value = float2.Normalize(foodPosition.Value - eaterPosition.Value) * 20;
                }
            }
        }

        public interface IEater : IComponentGroup
        {
            IComponentArray<Position> Positions { get; }
            IComponentArray<Velocity> Velocity { get; }
            IComponentArray<Score> Score { get; }
        }

        public interface IFood : IComponentGroup
        {
            IComponentArray<Position> Positions { get; }
            IComponentArray<PointValue> PointValue { get; }
        }
    }
}