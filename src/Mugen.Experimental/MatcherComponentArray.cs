namespace Mugen.Experimental
{
    using System;
    using System.Runtime.CompilerServices;
    using Abstraction;
    using Abstraction.BlueprintManager;

    internal abstract class AMatcherComponentArray
    {
        public abstract void Invalidate();
    }

    internal sealed class MatcherComponentArray<T> : AMatcherComponentArray, IComponentArray<T> where T : struct, IComponent
    {
        private readonly ComponentMatcher _componentMatcher;
        private int _blueprintIndex;


        private BlueprintInfo _infoForCurrentBlueprint;
        private readonly int _typeIndex;

        private int _chunkEnd;

        private unsafe BlueprintEntityChunk* _currentChunk;
        private unsafe void* _currentPointer;

        private int _currentStart;
        private int _currentEnd;

        private readonly int _size;

        private struct BlueprintInfo
        {
            public int Offset;
        }

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

        public override unsafe void Invalidate()
        {
            _currentStart = 0;
            _currentEnd = 0;
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
                _chunkEnd = _currentStart + _currentChunk->EntityCount;
                _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            }

            while (index >= _currentStart + _currentChunk->EntityCount)
            {
                _currentStart += _currentChunk->EntityCount;
                _currentChunk = _currentChunk->NextChunk;
                _chunkEnd = _currentStart + _currentChunk->EntityCount;
                _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            }
        }

        public unsafe ref T this[int index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(index < _currentStart || index >= _chunkEnd) Set(index);

                return ref Unsafe.AsRef<T>((byte*)_currentPointer + _size * (index - _currentStart));
            }
        }
    }
}