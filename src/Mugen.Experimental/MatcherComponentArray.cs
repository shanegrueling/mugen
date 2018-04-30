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
        private unsafe BlueprintEntityChunk* _currentChunk;
        private unsafe void* _currentPointer;

        private int _currentStart;
        private int _currentEnd;

        private struct BlueprintInfo
        {
            public int Offset;
        }

        public unsafe MatcherComponentArray(ComponentMatcher componentMatcher)
        {
            _componentMatcher = componentMatcher;
            _typeIndex = TypeManager.GetIndex(typeof(T));
            _infoForCurrentBlueprint = new BlueprintInfo();
            _currentChunk = null;
        }

        public override unsafe void Invalidate()
        {
            _blueprintIndex = -1;
            _currentStart = 0;
            _currentChunk = null;
            NextBlueprint();
        }

        private unsafe void NextBlueprint()
        {
            ++_blueprintIndex;
            if(_blueprintIndex >= _componentMatcher.AmountOfMatchedBlueprints) throw new ArgumentException();

            ref var blueprintData = ref *_componentMatcher.MatchedBlueprints[_blueprintIndex].BlueprintData;

            for (var i = 0; i < blueprintData.ComponentTypesCount; ++i)
            {
                if (blueprintData.ComponentTypes[i].TypeIndex == _typeIndex)
                {
                    _infoForCurrentBlueprint.Offset = blueprintData.Offsets[i];
                    break;
                }
            }
            NextChunk();
        }

        public unsafe ref T this[int index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_currentEnd < index)
                {
                    NextChunk();
                }
                return ref new Span<T>(_currentPointer, 1024)[index];
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        private unsafe void NextChunk()
        {
            if (_currentChunk == null)
            {
                FirstChunk(ref *_componentMatcher.MatchedBlueprints[_blueprintIndex].BlueprintData);
                return;
            }

            if (_currentChunk->NextChunk == null)
            {
                NextBlueprint();
                return;
            }
            
            ref var blueprintData = ref *_componentMatcher.MatchedBlueprints[_blueprintIndex].BlueprintData;

            _currentStart += _currentChunk->EntityCount;
            _currentChunk = blueprintData.FirstChunk;
            _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            _currentEnd = _currentStart + _currentChunk->EntityCount;
        }

        private unsafe void FirstChunk(ref BlueprintData blueprintData)
        {
            _currentStart = 0;
            _currentChunk = blueprintData.FirstChunk;
            _currentPointer = _currentChunk->Buffer + _infoForCurrentBlueprint.Offset;
            _currentEnd = _currentStart + _currentChunk->EntityCount;
        }
    }
}