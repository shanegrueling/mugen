namespace Mugen.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Abstraction;
    using Abstraction.BlueprintManager;

    internal class ComponentMatcher : IComponentMatcher, IDisposable
    {
        public unsafe struct MatchedBlueprintInfo
        {
            public BlueprintData* BlueprintData;
        }

        public unsafe MatchedBlueprintInfo* MatchedBlueprints;
        private int _matchedBlueprintArrayLength;
        public int AmountOfMatchedBlueprints;

        private readonly EntityArray _entityArray;
        private readonly AMatcherComponentArray[] _arrays;
        private readonly ComponentMatcherTypes[] _types;
        private int _length;

        public int Length => _length == -1 ? CalculateLength() : _length;

        private unsafe int CalculateLength()
        {
            _length = 0;
            for (var i = 0; i < AmountOfMatchedBlueprints; ++i)
            {
                _length += MatchedBlueprints[i].BlueprintData->EntityCount;
            }

            return _length;
        }

        private class ComponentMatcherTypesComparer : IComparer<ComponentMatcherTypes>
        {
            public int Compare(ComponentMatcherTypes x, ComponentMatcherTypes y)
            {
                return x.DataIndex - y.DataIndex;
            }
        }

        public unsafe ComponentMatcher(ComponentMatcherTypes[] types)
        {
            _types = types;
            Array.Sort(_types, new ComponentMatcherTypesComparer());

            MatchedBlueprints = (MatchedBlueprintInfo*) Marshal.AllocHGlobal(10 * sizeof(MatchedBlueprintInfo));
            AmountOfMatchedBlueprints = 0;
            _length = -1;
            _matchedBlueprintArrayLength = 10;

            _entityArray = new EntityArray(this);

            _arrays = new AMatcherComponentArray[_types.Length];
            var type = typeof(MatcherComponentArray<>);
            for (var i = 0; i < _arrays.Length; ++i)
            {
                var genericType = type.MakeGenericType(TypeManager.GetComponentType(_types[i].DataIndex).Type);

                _arrays[i] = (AMatcherComponentArray) Activator.CreateInstance(genericType, this);
            }
        }

        public bool DoesMatch(Span<ComponentType> componentTypes)
        {
            var j = 0;
            for (var i = 0; i < _types.Length; ++i)
            {
                var isFound = false;
                for (; j < componentTypes.Length; ++j)
                {
                    if (_types[i].DataIndex != componentTypes[j].DataIndex) continue;

                    isFound = true;
                    break;
                }

                if (!isFound) return false;
            }

            return true;
        }

        public unsafe void AddBlueprint(BlueprintData* blueprintData)
        {
            if (AmountOfMatchedBlueprints >= _matchedBlueprintArrayLength)
            {
                _matchedBlueprintArrayLength *= 2;
                MatchedBlueprints = (MatchedBlueprintInfo*) Marshal.ReAllocHGlobal(
                    (IntPtr) MatchedBlueprints,
                    (IntPtr) (_matchedBlueprintArrayLength * sizeof(MatchedBlueprintInfo)));
            }

            MatchedBlueprints[AmountOfMatchedBlueprints++] = new MatchedBlueprintInfo { BlueprintData = blueprintData };
            _length = -1;
        }

        public IEntityArray GetEntityArray() => _entityArray;

        public IComponentArray<T> GetComponentArray<T>() where T : unmanaged, IComponent
        {
            var index = TypeManager.GetIndex(typeof(T));
            for (var i = 0; i < _types.Length; ++i)
            {
                if (_types[i].DataIndex == index) return (MatcherComponentArray<T>)_arrays[i];
            }

            return null;
        }

        public unsafe void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)MatchedBlueprints);
            MatchedBlueprints = null;
        }

        public void Invalidate()
        {
            if (AmountOfMatchedBlueprints == 0) return;
            _length = -1;

            _entityArray.Invalidate();

            for (var i = 0; i < _arrays.Length; ++i)
            {
                _arrays[i].Invalidate();
            }
        }
    }
}