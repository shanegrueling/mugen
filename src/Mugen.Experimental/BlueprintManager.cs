namespace Mugen.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Abstraction;
    using Abstraction.BlueprintManager;

    internal unsafe class BlueprintManager : IDisposable
    {
        private BlueprintData* _lastBlueprintData;
        
        private readonly List<ComponentMatcher> _matchers;

        public BlueprintManager()
        {
            _lastBlueprintData = null;
            _matchers = new List<ComponentMatcher>();
        }

        public Blueprint GetOrCreateBlueprint(Span<ComponentType> types)
        {
            var blueprintComponentTypeArray = stackalloc BlueprintComponentType[types.Length];
            for (var i = 0; i < types.Length; ++i)
            {
                blueprintComponentTypeArray[i] = new BlueprintComponentType(types[i]);
            }

            var blueprintDataPointer = FindExistingBlueprint(blueprintComponentTypeArray, types.Length);

            if(blueprintDataPointer != null) return new Blueprint(blueprintDataPointer);

            return new Blueprint(CreateBlueprintData(blueprintComponentTypeArray, types.Length));
        }

        private BlueprintData* CreateBlueprintData(BlueprintComponentType* blueprintComponentTypeArray, int count)
        {
            var blueprintData = (BlueprintData*) Marshal.AllocHGlobal(sizeof(BlueprintData));
            blueprintData->ComponentTypesCount = count;
            var byteSize = count * sizeof(BlueprintComponentType);
            blueprintData->ComponentTypes = (BlueprintComponentType*) Marshal.AllocHGlobal(byteSize);
            Buffer.MemoryCopy(blueprintComponentTypeArray, blueprintData->ComponentTypes, byteSize, byteSize);
            
            blueprintData->Offsets = (int*) Marshal.AllocHGlobal(sizeof(int) * count);
            blueprintData->SizeOfs = (int*) Marshal.AllocHGlobal(sizeof(int) * count);

            var sizeOfInstance = sizeof(Entity);
            var offset = sizeof(Entity) * 1024;

            for (var i = 0; i < count; ++i)
            {
                var size = TypeManager.GetComponentType(blueprintComponentTypeArray[i].TypeIndex).Size;
                blueprintData->SizeOfs[i] = size;
                blueprintData->Offsets[i] = offset;
                
                offset += size * 1024;

                sizeOfInstance += size;
            }

            blueprintData->ChunkWithSpace = blueprintData->LastChunk = blueprintData->FirstChunk = BuildChunk(blueprintData, sizeOfInstance);

            blueprintData->PreviousBlueprintData = _lastBlueprintData;
            _lastBlueprintData = blueprintData;

            blueprintData->EntityCount = 0;

            foreach (var matcher in _matchers)
            {
                if (matcher.DoesMatch(
                    new Span<ComponentType>(blueprintData->ComponentTypes, blueprintData->ComponentTypesCount)))
                {
                    matcher.AddBlueprint(blueprintData);
                }
            }

            return _lastBlueprintData;
        }

        public static BlueprintEntityChunk* BuildChunk(BlueprintData* blueprintData)
        {
            var sizeOfInstance = sizeof(Entity);
            for (var i = 0; i < blueprintData->ComponentTypesCount; ++i)
            {
                sizeOfInstance += blueprintData->SizeOfs[i];
            }

            return BuildChunk(blueprintData, sizeOfInstance);
        }

        private static BlueprintEntityChunk* BuildChunk(BlueprintData* blueprintData, int sizeOfInstance)
        {
            var chunk = (BlueprintEntityChunk*) Marshal.AllocHGlobal(
                sizeof(BlueprintEntityChunk) + sizeOfInstance * 1024);

            chunk->Blueprint = blueprintData;
            chunk->Capacity = 1024;
            chunk->EntityCount = 0;
            chunk->ChunkWithSpace = null;
            chunk->NextChunk = null;
            
            return chunk;
        }

        private BlueprintData* FindExistingBlueprint(BlueprintComponentType* blueprintComponentTypeArray, int count)
        {
            var blueprintData = _lastBlueprintData;
            while (blueprintData != null)
            {
                if (blueprintData->ComponentTypesCount != count)
                {
                    blueprintData = blueprintData->PreviousBlueprintData;
                    continue;
                }

                var isSame = true;
                for (var i = 0; i < count; ++i)
                {
                    if (blueprintData->ComponentTypes[i].TypeIndex == blueprintComponentTypeArray[i].TypeIndex)
                    {
                        continue;
                    }

                    isSame = false;
                    break;
                }

                if (isSame)
                {
                    break;
                }

                blueprintData = blueprintData->PreviousBlueprintData;
            }

            return blueprintData;
        }

        public void Dispose()
        {
            while (_lastBlueprintData != null)
            {
                var next = _lastBlueprintData->PreviousBlueprintData;

                var chunk = _lastBlueprintData->FirstChunk;
                while (chunk != null)
                {
                    var nextChunk = chunk->NextChunk;

                    Marshal.FreeHGlobal((IntPtr)chunk);
                    chunk = nextChunk;
                }

                Marshal.FreeHGlobal((IntPtr)_lastBlueprintData->ComponentTypes);
                Marshal.FreeHGlobal((IntPtr)_lastBlueprintData->Offsets);
                Marshal.FreeHGlobal((IntPtr)_lastBlueprintData->SizeOfs);
                Marshal.FreeHGlobal((IntPtr)_lastBlueprintData);
                _lastBlueprintData = next;
            }
        }

        public void CheckBlueprintsForMatcher(ComponentMatcher matcher)
        {
            _matchers.Add(matcher);
            var blueprint = _lastBlueprintData;
            while (blueprint != null)
            {
                if (matcher.DoesMatch(
                    new Span<ComponentType>(blueprint->ComponentTypes, blueprint->ComponentTypesCount)))
                {
                    matcher.AddBlueprint(blueprint);
                }

                blueprint = blueprint->PreviousBlueprintData;
            }
        }
    }
}
