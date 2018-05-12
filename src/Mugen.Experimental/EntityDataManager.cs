namespace Mugen.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Abstraction;
    using Abstraction.BlueprintManager;

    internal unsafe class EntityDataManager : IDisposable
    {
        private readonly BlueprintManager _blueprintManager;

        private readonly Queue<int> _freeEntityDataSlots;
        private int _entityCount;

        private EntityData* _entityData;
        private int _entityDataLength;

        public EntityDataManager(BlueprintManager blueprintManager)
        {
            _blueprintManager = blueprintManager;
            _entityDataLength = 1024;
            _entityData = (EntityData*) Marshal.AllocHGlobal(sizeof(EntityData) * _entityDataLength);
            _freeEntityDataSlots = new Queue<int>();
            _entityCount = 0;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr) _entityData);
            _entityData = null;
        }

        public Entity CreateEntity()
        {
            var resetVersion = true;
            var index = _entityCount++;
            if (_freeEntityDataSlots.Count > 0)
            {
                index = _freeEntityDataSlots.Dequeue();
                --_entityCount;
                resetVersion = false;
            }

            if (_entityCount > _entityDataLength)
            {
                Resize();
            }

            ref var entityData = ref _entityData[index];
            entityData.BlueprintData = null;
            entityData.Version = resetVersion ? 0 : entityData.Version + 1;

            return new Entity(index, entityData.Version);
        }

        public Entity CreateEntity(Blueprint blueprint)
        {
            var resetVersion = true;
            int index;
            if (_freeEntityDataSlots.Count > 0)
            {
                index = _freeEntityDataSlots.Dequeue();
                resetVersion = false;
            }
            else
            {
                index = _entityCount++;
                if (_entityCount > _entityDataLength)
                {
                    Resize();
                }
            }

            ref var entityData = ref _entityData[index];
            entityData.BlueprintData = blueprint.BlueprintData;
            entityData.Version = resetVersion ? 0 : entityData.Version + 1;

            AllocateEntity(ref entityData, index);

            return new Entity(index, entityData.Version);
        }

        private void AllocateEntity(ref EntityData entityData, int index)
        {
            var chunk = entityData.BlueprintData->ChunkWithSpace;

            if (chunk == null)
            {
                chunk = BlueprintManager.BuildChunk(entityData.BlueprintData);
                entityData.BlueprintData->ChunkWithSpace = chunk;
                entityData.BlueprintData->LastChunk->NextChunk = chunk;
                entityData.BlueprintData->LastChunk = chunk;
            }

            entityData.Chunk = chunk;
            entityData.IndexInChunk = entityData.Chunk->EntityCount++;

            if (chunk->Capacity == chunk->EntityCount)
            {
                entityData.BlueprintData->ChunkWithSpace = chunk->ChunkWithSpace;
            }

            ++entityData.BlueprintData->EntityCount;

            ref var entityInChunk = ref ((Entity*) chunk->Buffer)[entityData.IndexInChunk];
            entityInChunk = new Entity(index, entityData.Version);
        }

        internal void SetComponent(Entity entity, int typeIndex, byte* pointer)
        {
            ref var data = ref _entityData[entity.DataIndex];

            var index = -1;
            for (var i = 0; i < data.BlueprintData->ComponentTypesCount; ++i)
            {
                if (data.BlueprintData->ComponentTypes[i].TypeIndex == typeIndex)
                {
                    index = i;
                    break;
                }
            }

            var offset = data.BlueprintData->Offsets[index];

            Unsafe.CopyBlock(data.Chunk->Buffer + offset, pointer, (uint) TypeManager.GetComponentType(typeIndex).Size);
        }

        private void Resize()
        {
            _entityDataLength *= 2;
            _entityData = (EntityData*) Marshal.ReAllocHGlobal(
                (IntPtr) _entityData,
                new IntPtr(sizeof(EntityData) * _entityDataLength));
        }

        public bool Exists(Entity entity)
        {
            ref var data = ref _entityData[entity.DataIndex];
            return data.Version == entity.Version && data.BlueprintData != null;
        }

        public ref T GetComponent<T>(Entity entity, int typeIndex)
        {
            ref var data = ref _entityData[entity.DataIndex];

            var index = -1;
            for (var i = 0; i < data.BlueprintData->ComponentTypesCount; ++i)
            {
                if (data.BlueprintData->ComponentTypes[i].TypeIndex == typeIndex)
                {
                    index = i;
                    break;
                }
            }

            var offset = data.BlueprintData->Offsets[index];

            var span = new Span<T>(data.Chunk->Buffer + offset, 1024);
            return ref span[data.IndexInChunk];
        }

        public void SetComponent<T>(Entity entity, int typeIndex, T component)
        {
            GetComponent<T>(entity, typeIndex) = component;
        }

        internal void AddComponent(Entity entity, int componentTypeIndex)
        {
            ref var data = ref _entityData[entity.DataIndex];

            if (data.BlueprintData == null)
            {
                Span<int> ctA = stackalloc int[1];
                ctA[0] = componentTypeIndex;
                var blueprint = _blueprintManager.GetOrCreateBlueprint(ctA);
                data.BlueprintData = blueprint.BlueprintData;

                AllocateEntity(ref data, entity.DataIndex);
                return;
            }

            var oldCt = new Span<BlueprintComponentType>(
                data.BlueprintData->ComponentTypes,
                data.BlueprintData->ComponentTypesCount);
            Span<int> newCt = stackalloc int[oldCt.Length + 1];

            var i = 0;
            for (; i < newCt.Length - 1; ++i)
            {
                if (oldCt[i].TypeIndex < componentTypeIndex)
                {
                    newCt[i] = oldCt[i].TypeIndex;
                }
                else
                {
                    break;
                }
            }

            newCt[i++] = componentTypeIndex;

            for (; i < newCt.Length; ++i)
            {
                newCt[i] = oldCt[i - 1].TypeIndex;
            }
            
            var newBlueprint = _blueprintManager.GetOrCreateBlueprint(newCt);

            if (newBlueprint.BlueprintData->ChunkWithSpace == null)
            {
                var chunk = BlueprintManager.BuildChunk(newBlueprint.BlueprintData);
                newBlueprint.BlueprintData->ChunkWithSpace = chunk;
                newBlueprint.BlueprintData->LastChunk->NextChunk = chunk;
                newBlueprint.BlueprintData->LastChunk = chunk;
            }

            CopyFromTo(
                data.Chunk,
                data.IndexInChunk,
                newBlueprint.BlueprintData->ChunkWithSpace,
                newBlueprint.BlueprintData->ChunkWithSpace->EntityCount);

            RemoveEntity(data.Chunk, data.IndexInChunk);
            
            data.BlueprintData = newBlueprint.BlueprintData;
            AllocateEntity(ref data, entity.DataIndex);
        }

        internal void AddComponent(Entity entity, int componentTypeIndex, byte* pointer)
        {
            AddComponent(entity, componentTypeIndex);
            SetComponent(entity, componentTypeIndex, pointer);
        }

        public void AddComponent<T>(in Entity entity) where T : unmanaged, IComponent
        {
            AddComponent(entity, TypeManager.GetIndex<T>());
        }

        private void RemoveEntity(BlueprintEntityChunk* chunk, int index)
        {
            var blueprint = chunk->Blueprint;

            if (chunk->EntityCount > index + 1)
            {
                var entitySize = sizeof(Entity);

                var amountOfEntitiesToMove = 1;
                var destination = chunk->Buffer + entitySize * index;
                var source = chunk->Buffer + entitySize * (chunk->EntityCount - 1);

                var entitiesSize = amountOfEntitiesToMove * entitySize;
                Buffer.MemoryCopy(source, destination, entitiesSize, entitiesSize);

                _entityData[((Entity*) destination)->DataIndex].IndexInChunk = index;

                for (var i = 0; i < blueprint->ComponentTypesCount; ++i)
                {
                    var sizeOf = blueprint->SizeOfs[i];

                    var destinationComponent = chunk->Buffer + blueprint->Offsets[i] + sizeOf * index;
                    var sourceComponent = chunk->Buffer + blueprint->Offsets[i] + sizeOf * (chunk->EntityCount - 1);

                    var size = amountOfEntitiesToMove * sizeOf;
                    Buffer.MemoryCopy(sourceComponent, destinationComponent, size, size);
                }
            }

            if (chunk->Capacity == chunk->EntityCount)
            {
                var lastFree = blueprint->ChunkWithSpace;
                while (lastFree != null)
                {
                    if (lastFree->ChunkWithSpace == null)
                    {
                        break;
                    }

                    lastFree = lastFree->ChunkWithSpace;
                }

                if (lastFree != null)
                {
                    lastFree->ChunkWithSpace = chunk;
                }
                else
                {
                    blueprint->ChunkWithSpace = chunk;
                }

                chunk->ChunkWithSpace = null;
            }

            blueprint->EntityCount -= 1;
            chunk->EntityCount -= 1;
        }

        private void RemoveEntityAll(BlueprintEntityChunk* chunk, int index)
        {
            var blueprint = chunk->Blueprint;

            if (chunk->EntityCount > index + 1)
            {
                var entitySize = sizeof(Entity);

                var amountOfEntitiesToMove = chunk->EntityCount - 1 - index;
                var destination = chunk->Buffer + entitySize * index;
                var source = destination + entitySize;

                var entitiesSize = amountOfEntitiesToMove * entitySize;
                Buffer.MemoryCopy(source, destination, entitiesSize, entitiesSize);

                var entities = (Entity*) destination;

                for (var i = 0; i < amountOfEntitiesToMove; ++i)
                {
                    _entityData[entities[i].DataIndex].IndexInChunk = index + i;
                }

                for (var i = 0; i < blueprint->ComponentTypesCount; ++i)
                {
                    var sizeOf = blueprint->SizeOfs[i];

                    var destinationComponent = chunk->Buffer + blueprint->Offsets[i] + sizeOf * index;
                    var sourceComponent = destinationComponent + sizeOf;

                    var size = amountOfEntitiesToMove * sizeOf;
                    Buffer.MemoryCopy(sourceComponent, destinationComponent, size, size);
                }
            }

            if (chunk->Capacity == chunk->EntityCount)
            {
                var lastFree = blueprint->ChunkWithSpace;
                while (lastFree != null)
                {
                    if (lastFree->ChunkWithSpace == null)
                    {
                        break;
                    }

                    lastFree = lastFree->ChunkWithSpace;
                }

                if (lastFree != null)
                {
                    lastFree->ChunkWithSpace = chunk;
                }
                else
                {
                    blueprint->ChunkWithSpace = chunk;
                }

                chunk->ChunkWithSpace = null;
            }

            blueprint->EntityCount -= 1;
            chunk->EntityCount -= 1;
        }

        private void CopyFromTo(
            BlueprintEntityChunk* sourceChunk,
            int sourceIndex,
            BlueprintEntityChunk* destinationChunk,
            int destinationIndex)
        {

            var sourceBlueprint = sourceChunk->Blueprint;
            var destinationBlueprint = destinationChunk->Blueprint;

            var sourceTypeIndex = 0;
            var destinationTypeIndex = 0;

            while (sourceTypeIndex < sourceBlueprint->ComponentTypesCount &&
                   destinationTypeIndex < destinationBlueprint->ComponentTypesCount)
            {
                if (sourceBlueprint->ComponentTypes[sourceTypeIndex].TypeIndex <
                    destinationBlueprint->ComponentTypes[destinationTypeIndex].TypeIndex)
                {
                    ++sourceTypeIndex;
                }
                else if (sourceBlueprint->ComponentTypes[sourceTypeIndex].TypeIndex >
                         destinationBlueprint->ComponentTypes[destinationTypeIndex].TypeIndex)
                {
                    ++destinationTypeIndex;
                }
                else
                {
                    var sourcePointer = sourceChunk->Buffer + sourceBlueprint->Offsets[sourceTypeIndex] +
                                        sourceBlueprint->SizeOfs[sourceTypeIndex] * sourceIndex;

                    var destinationPointer = destinationChunk->Buffer +
                                              destinationBlueprint->Offsets[destinationTypeIndex] +
                                              destinationBlueprint->SizeOfs[destinationTypeIndex] * destinationIndex;

                    Unsafe.CopyBlock(
                        destinationPointer,
                        sourcePointer,
                        (uint) destinationBlueprint->SizeOfs[destinationTypeIndex]);
                    ++sourceTypeIndex;
                    ++destinationTypeIndex;
                }
            }
        }

        public void AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            AddComponent<T>(entity);
            var typeIndex = TypeManager.GetIndex(typeof(T));
            SetComponent(entity, typeIndex, component);
        }

        public void DeleteEntity(in Entity entity)
        {
            ref var data = ref _entityData[entity.DataIndex];
            RemoveEntity(data.Chunk, data.IndexInChunk);
            data.Chunk = null;
            data.IndexInChunk = -1;
            ++data.Version;

            _freeEntityDataSlots.Enqueue(entity.DataIndex);
        }

        private struct EntityData
        {
            public BlueprintData* BlueprintData;
            public int Version;
            public BlueprintEntityChunk* Chunk;
            public int IndexInChunk;
        }

        public bool HasComponent(Entity entity, int typeIndex)
        {
            ref var data = ref _entityData[entity.DataIndex];
            for (var i = 0; i < data.BlueprintData->ComponentTypesCount; ++i)
            {
                if (data.BlueprintData->ComponentTypes[i].TypeIndex == typeIndex) return true;
            }

            return false;
        }

        public void RemoveComponent(in Entity entity, int typeIndex)
        {
            ref var data = ref _entityData[entity.DataIndex];
            Span<int> blueprintComponents =
                stackalloc int[data.BlueprintData->ComponentTypesCount - 1];
            
            for (int i = 0, newBlueprintComponentsIndex = 0; i < data.BlueprintData->ComponentTypesCount; ++i)
            {
                if(data.BlueprintData->ComponentTypes[i].TypeIndex == typeIndex) continue;

                blueprintComponents[newBlueprintComponentsIndex++] = data.BlueprintData->ComponentTypes[i].TypeIndex;
            }

            var newBlueprint = _blueprintManager.GetOrCreateBlueprint(blueprintComponents);

            if (newBlueprint.BlueprintData->ChunkWithSpace == null)
            {
                var chunk = BlueprintManager.BuildChunk(newBlueprint.BlueprintData);
                newBlueprint.BlueprintData->ChunkWithSpace = chunk;
                newBlueprint.BlueprintData->LastChunk->NextChunk = chunk;
                newBlueprint.BlueprintData->LastChunk = chunk;
            }
            
            CopyFromTo(
                data.Chunk,
                data.IndexInChunk,
                newBlueprint.BlueprintData->ChunkWithSpace,
                newBlueprint.BlueprintData->ChunkWithSpace->EntityCount);

            RemoveEntity(data.Chunk, data.IndexInChunk);
            
            data.BlueprintData = newBlueprint.BlueprintData;
            AllocateEntity(ref data, entity.DataIndex);
        }
    }
}