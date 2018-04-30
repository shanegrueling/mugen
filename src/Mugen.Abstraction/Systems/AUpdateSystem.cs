namespace Mugen.Abstraction.Systems
{
    public abstract class AUpdateSystem : IUpdateSystemBase
    {
        public bool IsAsync => false;

        public abstract void Update(float deltaTime);
    }
}
