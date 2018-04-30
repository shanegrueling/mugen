namespace Mugen.Experimental
{
    using Abstraction;

    internal class ComponentArray<T> : IComponentArray<T> where T : struct, IComponent
    {
        public ref T this[int index] => throw new System.NotImplementedException();
    }
}