﻿namespace Mugen
{
    using System.Runtime.CompilerServices;
    using Abstraction;
    using Abstraction.Arrays;

    internal abstract class AMatcherComponentArray
    {
        public abstract void Invalidate();
    }

    internal sealed class MatcherComponentArray<T> : AMatcherComponentArray, IComponentArray<T>
        where T : unmanaged, IComponent
    {
        private readonly ComponentMatcher _componentMatcher;

        private readonly int _size;
        private readonly int _typeIndex;
        private int _blueprintIndex;

        private int _chunkEnd;

        private unsafe BlueprintEntityChunk* _currentChunk;
        private int _currentEnd;
        private unsafe byte* _currentPointer;

        private int _currentStart;


        private BlueprintInfo _infoForCurrentBlueprint;

        public unsafe MatcherComponentArray(ComponentMatcher componentMatcher)
        {
            _componentMatcher = componentMatcher;
            _typeIndex = TypeManager.GetIndex<T>();
            _infoForCurrentBlueprint = new BlueprintInfo();
            _size = Unsafe.SizeOf<T>();
            _currentStart = 0;
            _currentEnd = 0;
            _currentChunk = null;
            _blueprintIndex = -1;
        }

        public unsafe ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var localIndex = index - _currentStart;
                if ((uint) localIndex >= (uint) _chunkEnd)
                {
                    Set(index);
                    localIndex = index - _currentStart;
                }

                return ref Unsafe.AsRef<T>(_currentPointer + _size * localIndex);
            }
        }

        public override unsafe void Invalidate()
        {
            _currentStart = 0;
            _currentEnd = 0;
            _chunkEnd = 0;
            _currentPointer = null;
            _currentChunk = null;
            _blueprintIndex = -1;
        }

        private unsafe void Set(int index)
        {
            if (index < _currentStart)
            {
                _currentStart = 0;
                _currentEnd = 0;
                _currentChunk = null;
                _blueprintIndex = -1;
            }

            while (index >= _currentEnd)
            {
                _currentStart = _currentEnd;
                ++_blueprintIndex;
                ref var blueprintData = ref *_componentMatcher.MatchedBlueprints[_blueprintIndex].BlueprintData;
                for (var i = 0; i < blueprintData.ComponentTypesCount; ++i)
                {
                    if (blueprintData.ComponentTypes[i].TypeIndex == _typeIndex)
                    {
                        _infoForCurrentBlueprint.Offset = blueprintData.Offsets[i];
                    }
                }

                _currentEnd = _currentStart + blueprintData.EntityCount;
                _currentChunk = blueprintData.FirstChunk;
                _chunkEnd = _currentChunk->EntityCount;
                _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            }

            while (index >= _currentStart + _currentChunk->EntityCount)
            {
                _currentStart += _currentChunk->EntityCount;
                _currentChunk = _currentChunk->NextChunk;
                _chunkEnd = _currentChunk->EntityCount;
                _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            }
        }

        private struct BlueprintInfo
        {
            public int Offset;
        }
    }
}