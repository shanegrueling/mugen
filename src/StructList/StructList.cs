namespace StructList
{
    using System;
    using System.Collections.Generic;

    public sealed class StructList<T> where T : struct
    {
        private const int DefaultCapacity = 4;
        private static readonly T[] EmptyArray = new T[0];

        private T[] _items;

        public StructList()
        {
            _items = EmptyArray;
        }

        public StructList(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(capacity),
                    capacity,
                    "The capacity can not be smaller than 0.");
            }

            _items = capacity == 0 ? EmptyArray : new T[capacity];
        }

        public StructList(IEnumerable<T> collection)
        {
            switch (collection)
            {
                case null:
                    throw new ArgumentNullException(nameof(collection));
                case ICollection<T> c:
                    var count = c.Count;
                    if (count == 0)
                    {
                        _items = EmptyArray;
                    }
                    else
                    {
                        _items = new T[count];
                        c.CopyTo(_items, 0);
                        Count = count;
                    }

                    break;
                default:
                    Count = 0;
                    _items = EmptyArray;

                    using (var en = collection.GetEnumerator())
                    {
                        while (en.MoveNext())
                        {
                            Add(en.Current);
                        }
                    }

                    break;
            }
        }

        public int Capacity
        {
            get => _items.Length;
            set
            {
                if (value < Count)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Capacity can't be reduced. Current Size: {Count}");
                }

                if (value == _items.Length)
                {
                    return;
                }

                if (value > 0)
                {
                    var newItems = new T[value];
                    if (Count > 0)
                    {
                        Array.Copy(_items, 0, newItems, 0, Count);
                    }

                    _items = newItems;
                }
                else
                {
                    _items = EmptyArray;
                }
            }
        }

        public ref T this[int index]
        {
            get
            {
                if ((uint) index >= (uint) Count)
                {
                    ThrowHelper.ThrowArgumentOutOfRangeException();
                }

                return ref _items[index];
            }
        }

        public int Count { get; private set; }

        public void Add(T item)
        {
            if (Count == _items.Length)
            {
                EnsureCapacity(Count + 1);
            }

            _items[Count++] = item;
        }

        public void Add()
        {
            if (Count == _items.Length)
            {
                EnsureCapacity(Count+1);
            }

            ++Count;
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min)
            {
                return;
            }

            var newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if (newCapacity < min)
            {
                newCapacity = min;
            }

            Capacity = newCapacity;
        }

        public void RemoveAt(int index)
        {
            if ((uint) index >= (uint) Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "The index was to big.");
            }

            --Count;
            if (index < Count)
            {
                Array.Copy(_items, index + 1, _items, index, Count - index);
            }

            _items[Count] = default(T);
        }

        public int IndexOf(T item)
        {
            return Array.IndexOf(_items, item, 0, Count);
        }
    }
}