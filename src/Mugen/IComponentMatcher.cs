namespace Mugen
{
    public interface IComponentMatcher
    {
        int Length { get; }

        IComponentArray<T> GetComponentArray<T>() where T : struct, IComponent;
    }
}
