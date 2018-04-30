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
        public int MatchedBlueprintArrayLength;
        public int AmountOfMatchedBlueprints;

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
                return y.DataIndex - x.DataIndex;
            }
        }

        public unsafe ComponentMatcher(ComponentMatcherTypes[] types)
        {
            _types = types;
            Array.Sort(_types, new ComponentMatcherTypesComparer());

            MatchedBlueprints = (MatchedBlueprintInfo*) Marshal.AllocHGlobal(10 * sizeof(MatchedBlueprintInfo));
            AmountOfMatchedBlueprints = 0;
            MatchedBlueprintArrayLength = 10;
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
            if (AmountOfMatchedBlueprints >= MatchedBlueprintArrayLength)
            {
                MatchedBlueprintArrayLength *= 2;
                MatchedBlueprints = (MatchedBlueprintInfo*) Marshal.ReAllocHGlobal(
                    (IntPtr) MatchedBlueprints,
                    (IntPtr) (MatchedBlueprintArrayLength * sizeof(MatchedBlueprintInfo)));
            }

            MatchedBlueprints[AmountOfMatchedBlueprints++] = new MatchedBlueprintInfo { BlueprintData = blueprintData };

            _length += blueprintData->EntityCount;
        }

        public IEntityArray GetEntityArray()
        {
            return null;
        }

        public IComponentArray<T> GetComponentArray<T>() where T : struct, IComponent
        {
            return null;
        }

        public unsafe void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)MatchedBlueprints);
            MatchedBlueprints = null;
        }

        public void Invalidate()
        {
            _length = -1;
        }
    }
}