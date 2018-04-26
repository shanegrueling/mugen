namespace Mugen
{
    public interface IComponentArray<T> where T : struct, IComponent
    {
        int Count { get; }
        ref T this[int index] { get; }
    }
}