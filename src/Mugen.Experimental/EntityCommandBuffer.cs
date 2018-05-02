namespace Mugen.Experimental
{
    using System;
    using Abstraction;
    using Abstraction.CommandBuffer;
    using Abstraction.CommandBuffers;

    internal sealed class EntityCommandBuffer : IEntityCommandBuffer
    {
        internal unsafe struct CommandChunk
        {
            public CommandChunk* Next;
            public CommandChunk* Previous;

            public int Used;
            public int Size;
        }

        private readonly EntityManager _manager;
        private unsafe CommandChunk* _first;
        private unsafe CommandChunk* _last;

        public EntityCommandBuffer(EntityManager manager)
        {
            _manager = manager;
        }

        public void CreateEntity(Blueprint blueprint)
        {
            throw new NotImplementedException();
        }

        public void CreateEntity()
        {
            throw new NotImplementedException();
        }

        public void AddComponent<T>(in Entity entity)
        {
            throw new NotImplementedException();
        }

        public void AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            throw new NotImplementedException();
        }

        public void ReplaceComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            throw new NotImplementedException();
        }

        public void SetComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            throw new NotImplementedException();
        }

        public void RemoveComponent<T>(in Entity entity) where T : struct, IComponent
        {
            throw new NotImplementedException();
        }

        public void DeleteEntity(in Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Playback()
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class EntityCommandBuffer<T> : IEntityCommandBuffer<T>
    {
        private readonly EntityCommandBuffer _commandBuffer;

        public EntityCommandBuffer(EntityManager manager)
        {
            _commandBuffer = new EntityCommandBuffer(manager);
        }

        public INewEntityCommandBuffer<T> CreateEntity(Blueprint blueprint)
        {
            return new NewEntityCommandBuffer(this);
        }

        public INewEntityCommandBuffer<T> CreateEntity()
        {
            return new NewEntityCommandBuffer(this);
        }

        public IEntityCommandBuffer<T> AddComponent<T1>(in Entity entity)
        {
            _commandBuffer.AddComponent<T1>(entity);
            return this;
        }

        public IEntityCommandBuffer<T> AddComponent<T1>(in Entity entity, in T1 component) where T1 : struct, IComponent
        {
            _commandBuffer.AddComponent(entity, component);
            return this;
        }

        public IEntityCommandBuffer<T> ReplaceComponent<T1>(in Entity entity, in T1 component)
            where T1 : struct, IComponent
        {
            _commandBuffer.ReplaceComponent(entity, component);
            return this;
        }

        public IEntityCommandBuffer<T> SetComponent<T1>(in Entity entity, in T1 component) where T1 : struct, IComponent
        {
            _commandBuffer.SetComponent(entity, component);
            return this;
        }

        public IEntityCommandBuffer<T> RemoveComponent<T1>(in Entity entity) where T1 : struct, IComponent
        {
            _commandBuffer.RemoveComponent<T1>(entity);
            return this;
        }

        public IEntityCommandBuffer<T> DeleteEntity(in Entity entity)
        {
            _commandBuffer.DeleteEntity(entity);
            return this;
        }

        public void Playback()
        {
            _commandBuffer.Playback();
        }

        private sealed class NewEntityCommandBuffer : INewEntityCommandBuffer<T>
        {
            private readonly EntityCommandBuffer<T> _entityCommandBuffer;

            public NewEntityCommandBuffer(EntityCommandBuffer<T> entityCommandBuffer)
            {
                _entityCommandBuffer = entityCommandBuffer;
            }

            public INewEntityCommandBuffer<T> SetComponent<T1>(in T1 component) where T1 : struct, IComponent
            {
                throw new NotImplementedException();
            }

            public IEntityCommandBuffer<T> Finish()
            {
                return _entityCommandBuffer;
            }
        }
    }
}