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

        private struct EntityData
        {
            public BlueprintData* BlueprintData;
            public int Version;
            public BlueprintEntityChunk* Chunk;
            public int IndexInChunk;
        }

        private EntityData* _entityData;
        private int _entityDataLength;
        private int _entityCount;

        private readonly Queue<int> _freeEntityDataSlots;

        public EntityDataManager(BlueprintManager blueprintManager)
        {
            _blueprintManager = blueprintManager;
            _entityDataLength = 1024;
            _entityData = (EntityData*) Marshal.AllocHGlobal(sizeof(EntityData) * _entityDataLength);
            _freeEntityDataSlots = new Queue<int>();
            _entityCount = 0;
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

            if (chunk->Capacity == entityData.Chunk->EntityCount)
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

            Unsafe.CopyBlock(data.Chunk->Buffer + offset, pointer, (uint)TypeManager.GetComponentType(typeIndex).Size);
        }

        private void Resize()
        {
            _entityDataLength *= 2;
            _entityData = (EntityData*)Marshal.ReAllocHGlobal((IntPtr) _entityData, new IntPtr(sizeof(EntityData) * _entityDataLength));
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)_entityData);
            _entityData = null;
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

        public void AddComponent<T>(in Entity entity) where T : struct, IComponent
        {
            ref var data = ref _entityData[entity.DataIndex];
            var ct = (ComponentType)typeof(T);

            if (data.BlueprintData == null)
            {
                Span<ComponentType> ctA = stackalloc ComponentType[1];
                ctA[0] = ct;
                var blueprint = _blueprintManager.GetOrCreateBlueprint(ctA);
                data.BlueprintData = blueprint.BlueprintData;

                AllocateEntity(ref data, entity.DataIndex);
                return;
            }

            var oldCt = new Span<BlueprintComponentType>(data.BlueprintData->ComponentTypes, data.BlueprintData->ComponentTypesCount);
            Span<ComponentType> newCt = stackalloc ComponentType[oldCt.Length + 1];

            for (var i = 0; i < newCt.Length; ++i)
            {
                if (oldCt[i].TypeIndex < ct.DataIndex)
                {
                    newCt[i] = TypeManager.GetComponentType(oldCt[i].TypeIndex).Type;
                }
            }

            var newBlueprint = _blueprintManager.GetOrCreateBlueprint(newCt);
            CopyFromTo(data.Chunk, data.IndexInChunk, newBlueprint.BlueprintData->ChunkWithSpace, newBlueprint.BlueprintData->ChunkWithSpace->EntityCount);

            RemoveEntity(data.Chunk, data.IndexInChunk);

            data.Chunk = newBlueprint.BlueprintData->ChunkWithSpace;
            data.IndexInChunk = newBlueprint.BlueprintData->ChunkWithSpace->EntityCount - 1;
        }

        private void RemoveEntity(BlueprintEntityChunk* chunk, int index)
        {
            var blueprint = chunk->Blueprint;
            
            if (chunk->EntityCount > index+1)
            {
                var amountOfEntitiesToMove = chunk->EntityCount - index;
                var destination = chunk->Buffer + sizeof(Entity) * index;
                var source = destination + sizeof(Entity);

                Unsafe.CopyBlock(destination, source, (uint)(amountOfEntitiesToMove * sizeof(Entity)));

                var span = new Span<Entity>(destination, amountOfEntitiesToMove);
                for (var i = 0; i < span.Length; ++i)
                {
                    _entityData[span[i].DataIndex].IndexInChunk = index + i;
                }

                for (var i = 0; i < blueprint->ComponentTypesCount; ++i)
                {
                    var destinationComponent = chunk->Buffer + blueprint->Offsets[i] + blueprint->SizeOfs[i] * index;
                    var sourceComponent = destinationComponent + blueprint->SizeOfs[i];

                    Unsafe.CopyBlock(destinationComponent, sourceComponent, (uint)(amountOfEntitiesToMove * blueprint->SizeOfs[i]));
                }
            }

            if (chunk->Capacity == chunk->EntityCount)
            {
                var lastFree = blueprint->ChunkWithSpace;
                while (lastFree != null)
                {
                    if (lastFree->ChunkWithSpace == null) break;

                    lastFree = lastFree->ChunkWithSpace;
                }

                lastFree->ChunkWithSpace = chunk;
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

            while (sourceTypeIndex < sourceBlueprint->ComponentTypesCount && destinationTypeIndex < destinationBlueprint->ComponentTypesCount)
            {
                if (sourceBlueprint->ComponentTypes[sourceTypeIndex].TypeIndex <
                    destinationBlueprint->ComponentTypes[destinationTypeIndex].TypeIndex)
                {
                    ++sourceTypeIndex;
                } else if (sourceBlueprint->ComponentTypes[sourceTypeIndex].TypeIndex >
                           destinationBlueprint->ComponentTypes[destinationTypeIndex].TypeIndex)
                {
                    ++destinationTypeIndex;
                }
                else
                {
                    var sourcePointer = sourceChunk->Buffer + 
                                        sourceBlueprint->Offsets[sourceTypeIndex] +
                                        sourceBlueprint->SizeOfs[sourceTypeIndex] * sourceIndex;

                    var destionationPointer = destinationChunk->Buffer +
                                              destinationBlueprint->Offsets[destinationTypeIndex] +
                                              destinationBlueprint->SizeOfs[destinationTypeIndex] * destinationIndex;
                    
                    Unsafe.CopyBlock(sourcePointer, destionationPointer, (uint)destinationBlueprint->SizeOfs[destinationTypeIndex]);
                    ++sourceTypeIndex;
                    ++destinationTypeIndex;
                }
            }
        }

        public void AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent
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
    }
}