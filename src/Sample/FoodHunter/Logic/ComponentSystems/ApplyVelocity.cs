namespace Sample.FoodHunter.Logic.ComponentSystems
{
    using Components;

    public static class ApplyVelocity
    {
        public static void Update(float deltaTime, ref Position position, ref Velocity velocity)
        {
            position.Value += velocity.Value * deltaTime;
        }
    }
}
