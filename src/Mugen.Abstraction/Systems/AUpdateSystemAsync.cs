namespace Mugen.Abstraction.Systems
{
    using System.Threading.Tasks;

    public abstract class AUpdateSystemAsync : IUpdateSystemBase
    {
        public bool IsAsync => true;

        public abstract Task Update(float deltaTime);
        
    }
}