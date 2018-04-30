namespace Mugen.Abstraction
{
    public interface IComponentMatcher
    {
        int Length { get; }
        
        IEntityArray GetEntityArray();
        IComponentArray<T> GetComponentArray<T>() where T : struct, IComponent;
    }

    public interface IComponentGroup
    {
        int Length { get; }
    }
}