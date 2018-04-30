namespace Mugen.Abstraction
{
    public interface IComponentArray<T>
    {
        ref T this[int index] { get; }
    }
}