namespace Mugen.Benchmark.ComponentSystems
{
    using Components;

    internal static class Move
    {
        public static void Update(float deltaTime, IMover components)
        {
            var l = components.Length;
            var p = components.Positions;
            var v = components.Velocities;
            for(var i = 0; i < l; ++i)
            {
                ref var position = ref p[i];
                ref var velocity = ref v[i];

                position.Value = position.Value + velocity.Value;
            }
        }
        public static void Update(float deltaTime, int length, IComponentArray<Position> positions, IComponentArray<Velocity> velocities)
        {
            for(var i = 0; i < length; ++i)
            {
                ref var position = ref positions[i];
                ref var velocity = ref velocities[i];

                position.Value = position.Value + velocity.Value;
            }
        }

        public static void Update(float deltaTime, ref Position position, ref Velocity velocity)
        {
            position.Value = position.Value + velocity.Value;
        }

        public interface IMover : IComponentGroup
        {
            IComponentArray<Position> Positions { get; }
            IComponentArray<Velocity> Velocities{ get; }
        }
    }
}

