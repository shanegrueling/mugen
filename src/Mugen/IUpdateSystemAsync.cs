namespace Mugen
{
    using System.Threading.Tasks;

    public interface IUpdateSystemAsync : IUpdateSystemBase
    {
        Task Update(float deltaTime);
    }
}