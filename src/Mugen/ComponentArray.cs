namespace Mugen
{
    using System;
    using System.Buffers;
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

    public sealed class ComponentArrayPool<T> : IComponentArray<T>, IComponentArray where T : struct, IComponent
    {
        private T[] _components;

        public ComponentArrayPool()
        {
            _components = ArrayPool<T>.Shared.Rent(0);
        }

        public ComponentArrayPool(int capacity)
        {
            _components = ArrayPool<T>.Shared.Rent(capacity);
        }

        public void Add(T component)
        {
            EnsureCapacity(Count + 1);
            _components[Count++] = component;
        }

        private void EnsureCapacity(int count)
        {
            if(count < _components.Length) return;

            var newItems = ArrayPool<T>.Shared.Rent(Count * 2);
            Array.Copy(_components, newItems, Count);
            ArrayPool<T>.Shared.Return(_components, true);

            _components = newItems;
        }

        public void RemoveAt(int index)
        {
            --Count;
            if (index < Count)
            {
                Array.Copy(_components, index + 1, _components, index, Count - index);
            }

            _components[Count] = default(T);
        }

        public void CreateNew()
        {
            EnsureCapacity(Count + 1);
            ++Count;
        }

        public int Count { get; private set; }

        public ref T this[int index] => ref _components[index];
    }
}