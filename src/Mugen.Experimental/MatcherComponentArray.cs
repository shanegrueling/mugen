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
        private bool _isInvalid;

        private struct BlueprintInfo
        {
            public int Offset;
        }

        public MatcherComponentArray(ComponentMatcher componentMatcher)
        {
            _componentMatcher = componentMatcher;
            _typeIndex = TypeManager.GetIndex<T>();
            _infoForCurrentBlueprint = new BlueprintInfo();
        }

        public override void Invalidate()
        {
            Set(0);
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
                if (_componentMatcher.AmountOfMatchedBlueprints <= _blueprintIndex)
                {
                    return;
                }
                ref var blueprintData = ref *_componentMatcher.MatchedBlueprints[_blueprintIndex].BlueprintData;
                _currentEnd = _currentStart + blueprintData.EntityCount;
                _currentChunk = blueprintData.FirstChunk;
            }

            while (index >= _currentStart + _currentChunk->EntityCount)
            {
                _currentStart += _currentChunk->EntityCount;
                _currentChunk = _currentChunk->NextChunk;
            }

            _currentEnd = _currentStart + _currentChunk->EntityCount;
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

            _currentChunk = null;
            NextChunk();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Reset()
        {
            Invalidate();
            _isInvalid = false;
            NextBlueprint();
        }

        public unsafe ref T this[int index] 
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if(index < _currentStart || index >= _currentEnd) Set(index);

                return ref Unsafe.AsRef<T>(Unsafe.Add<T>(_currentPointer, index - _currentStart));
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

            _currentStart += _currentChunk->EntityCount;
            _currentChunk = _currentChunk->NextChunk;
            _currentPointer = (_currentChunk->Buffer + _infoForCurrentBlueprint.Offset);
            _currentEnd = _currentStart + _currentChunk->EntityCount;
        }

        private unsafe void FirstChunk(ref BlueprintData blueprintData)
        {
            _currentChunk = blueprintData.FirstChunk;
            _currentPointer = (_currentChunk->Buffer + _infoForCurrentBlueprint.Offset);
            _currentEnd = _currentStart + _currentChunk->EntityCount;
        }
    }
}