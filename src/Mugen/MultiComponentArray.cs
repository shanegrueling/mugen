namespace Mugen
{
    using System.Collections.Generic;
    using StructList;

    internal class MultiComponentArray<T> : IMultiComponentArray, IComponentArray<T> where T : struct, IComponent
    {
        private readonly LinkedList<ComponentArrayPool<T>> _arrays;

        private LinkedListNode<ComponentArrayPool<T>> _currentArray;
        private int _startCurrentArray;
        private readonly EntityManager _manager;
        private int _version;
        private int _length;

        public int Count
        {
            get
            {
                if(_length > -1) return _length;

                var c = 0;
                foreach(var array in _arrays)
                {
                    c += array.Count;
                }

                _length = c;
                return c;
            }
        }

        public MultiComponentArray(EntityManager manager)
        {
            _manager = manager;
            _version = manager?.Version ?? 0;
            _length = -1;
            _arrays = new LinkedList<ComponentArrayPool<T>>();
        }

        public void Add(object componentArray)
        {
            _arrays.AddLast((ComponentArrayPool<T>)componentArray);

            if(_currentArray == null) _currentArray = _arrays.First;
        }

        private void Reset()
        {
            _currentArray = _arrays.First;
            _startCurrentArray = 0;
            _length = -1;
            _version = _manager?.Version ?? 0;
        }

        public ref T this[int index]
        {
            get
            {
                if((_manager?.Version ?? 0) != _version) Reset();
                if(_currentArray == null) ThrowHelper.ThrowArgumentOutOfRangeException();

                var localIndex = index - _startCurrentArray;

                if(localIndex < 0)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    _currentArray = _currentArray.Previous;
                    // ReSharper disable once PossibleNullReferenceException
                    _startCurrentArray -= _currentArray.Value.Count;

                    return ref this[index];
                }

                // ReSharper disable once PossibleNullReferenceException
                var c = _currentArray.Value.Count;

                if(localIndex < c)
                {
                    return ref _currentArray.Value[localIndex];
                }

                _startCurrentArray += c;
                _currentArray = _currentArray.Next;
            
                return ref this[index];
            }
        }
    }
}