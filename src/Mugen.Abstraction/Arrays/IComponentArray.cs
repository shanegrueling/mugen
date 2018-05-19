namespace Mugen.Abstraction.Arrays
{
    public interface IComponentArray<T>
    {
        ref T this[int index] { get; }
    }
}