namespace Mugen.Experimental
{
    using System.Runtime.CompilerServices;
    using Abstraction;

    internal sealed class EntityArray : IEntityArray
    {
        private readonly ComponentMatcher _componentMatcher;
        private int _blueprintIndex;

        private readonly BlueprintInfo _infoForCurrentBlueprint;

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

        public unsafe EntityArray(ComponentMatcher componentMatcher)
        {
            _componentMatcher = componentMatcher;

            _infoForCurrentBlueprint = new BlueprintInfo {Offset = 0};
            _size = Unsafe.SizeOf<Entity>();
            _currentStart = 0;
            _currentEnd = 0;
            _currentChunk = null;
            _blueprintIndex = -1;
        }

        public unsafe void Invalidate()
        {
            _currentStart = 0;
            _currentEnd = 0;
            _chunkEnd = 0;
            _currentChunk = null;
            _blueprintIndex = -1;
            _currentPointer = null;
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
                _currentEnd = _currentStart + blueprintData.EntityCount;
                _currentChunk = blueprintData.FirstChunk;
                _chunkEnd = _currentStart + _currentChunk->EntityCount;
                _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            }

            while (index >= _chunkEnd)
            {
                _currentStart += _currentChunk->EntityCount;
                _currentChunk = _currentChunk->NextChunk;
                _chunkEnd = _currentStart + _currentChunk->EntityCount;
                _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            }
        }

        public unsafe ref Entity this[int index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(index < _currentStart || index >= _chunkEnd) Set(index);

                return ref Unsafe.AsRef<Entity>((byte*)_currentPointer + _size * (index - _currentStart));
            }
        }
    }
}