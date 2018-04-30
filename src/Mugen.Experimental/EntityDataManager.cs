namespace Mugen.Experimental
{
    using System;
    using System.Collections.Generic;
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

            if (data.BlueprintData == null)
            {
                var blueprint = _blueprintManager.GetOrCreateBlueprint(new ComponentType[] {typeof(T)});
                data.BlueprintData = blueprint.BlueprintData;

                AllocateEntity(ref data, entity.DataIndex);
                return;
            }
        }

        public void AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            AddComponent<T>(entity);
            var typeIndex = TypeManager.GetIndex(typeof(T));
            SetComponent(entity, typeIndex, component);
        }
    }
}