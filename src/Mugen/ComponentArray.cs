namespace Mugen
{
    using StructList;

    public sealed class ComponentArray<T> : IComponentArray<T>, IComponentArray where T : struct, IComponent
    {
        private readonly StructList<T> _components;

        public ComponentArray()
        {
            _components = new StructList<T>();
        }

        public ComponentArray(int capacity)
        {
            _components = new StructList<T>(capacity);
        }

        public void Add(T component)
        {
            _components.Add(component);
        }

        public void RemoveAt(int index)
        {
            _components.RemoveAt(index);
        }

        public int Count => _components.Count;

        public ref T this[int index] => ref _components[index];
        public void CreateNew()
        {
            _components.Add();
        }
    }
}