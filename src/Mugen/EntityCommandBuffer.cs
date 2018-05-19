namespace Mugen
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Abstraction;
    using Abstraction.CommandBuffers;

    internal sealed class EntityCommandBuffer : IEntityCommandBuffer
    {
        private readonly EntityManager _manager;
        private unsafe CommandChunk* _first;
        private unsafe CommandChunk* _last;

        public unsafe EntityCommandBuffer(EntityManager manager)
        {
            _manager = manager;
            _first = null;
            _last = null;
        }

        public void CreateEntity(in Blueprint blueprint)
        {
            ref var createCommand = ref GetPointer<CreateCommand>(0);

            createCommand.Header.CommandType = Commands.Create;
            createCommand.Header.Size = Unsafe.SizeOf<CreateCommand>();
            createCommand.Blueprint = blueprint;
        }

        public unsafe void CreateEntity()
        {
            CreateEntity(new Blueprint(null));
        }

        public unsafe void AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            var componentSize = Unsafe.SizeOf<T>();

            ref var addCommand = ref GetPointer<AddCommand>(componentSize);

            addCommand.Header.CommandType = Commands.Add;
            addCommand.Header.Size = sizeof(AddCommand) + componentSize;
            addCommand.Entity = entity;
            addCommand.ComponentSize = componentSize;
            addCommand.ComponentTypeIndex = TypeManager.GetIndex<T>();

            Unsafe.AsRef<T>((byte*) Unsafe.AsPointer(ref addCommand) + sizeof(AddCommand)) = component;
        }

        public unsafe void ReplaceComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            var componentSize = Unsafe.SizeOf<T>();

            ref var replaceCommand = ref GetPointer<ReplaceCommand>(componentSize);

            replaceCommand.Header.CommandType = Commands.Replace;
            replaceCommand.Header.Size = sizeof(ReplaceCommand) + componentSize;
            replaceCommand.Entity = entity;
            replaceCommand.ComponentSize = componentSize;
            replaceCommand.ComponentTypeIndex = TypeManager.GetIndex<T>();

            Unsafe.AsRef<T>((byte*) Unsafe.AsPointer(ref replaceCommand) + sizeof(ReplaceCommand)) = component;
        }

        public unsafe void SetComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            var componentSize = Unsafe.SizeOf<T>();

            ref var setCommand = ref GetPointer<SetNewCommand>(componentSize);

            setCommand.Header.CommandType = Commands.Set;
            setCommand.Header.Size = sizeof(SetNewCommand) + componentSize;
            setCommand.Entity = entity;
            setCommand.ComponentSize = componentSize;
            setCommand.ComponentTypeIndex = TypeManager.GetIndex<T>();

            Unsafe.AsRef<T>((byte*) Unsafe.AsPointer(ref setCommand) + sizeof(SetNewCommand)) = component;
        }

        public unsafe void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
        {
            ref var removeCommand = ref GetPointer<RemoveCommand>(0);

            removeCommand.Header.CommandType = Commands.Remove;
            removeCommand.Header.Size = sizeof(RemoveCommand);
            removeCommand.Entity = entity;
            removeCommand.ComponentTypeIndex = TypeManager.GetIndex<T>();
        }

        public unsafe void DeleteEntity(in Entity entity)
        {
            ref var removeCommand = ref GetPointer<DeleteCommand>(0);

            removeCommand.Header.CommandType = Commands.Delete;
            removeCommand.Header.Size = sizeof(DeleteCommand);
            removeCommand.Entity = entity;
        }

        public unsafe void Playback()
        {
            var sizeOfChunk = sizeof(CommandChunk);

            var chunk = _first;
            _last = _first = null;
            var lastEntity = default(Entity);

            while (chunk != null)
            {
                var offset = 0;
                var bufferPointer = (byte*) chunk + sizeOfChunk;
                while (offset < chunk->Used)
                {
                    var commandPointer = (CommandHeader*) (bufferPointer + offset);
                    switch (commandPointer->CommandType)
                    {
                        case Commands.Create:
                            var createCommand = (CreateCommand*) commandPointer;
                            lastEntity = createCommand->Blueprint.BlueprintData != null
                                ? _manager.CreateEntity(createCommand->Blueprint)
                                : _manager.CreateEntity();
                            break;
                        case Commands.Add:
                            var addCommand = (AddCommand*) commandPointer;
                            _manager.AddComponent(
                                addCommand->Entity.DataIndex == -1 ? lastEntity : addCommand->Entity,
                                addCommand->ComponentTypeIndex,
                                (byte*) addCommand + sizeof(AddCommand));
                            break;
                        case Commands.Replace:
                            var replaceCommand = (ReplaceCommand*) commandPointer;
                            _manager.ReplaceComponent(
                                replaceCommand->Entity.DataIndex == -1 ? lastEntity : replaceCommand->Entity,
                                replaceCommand->ComponentTypeIndex,
                                (byte*) replaceCommand + sizeof(ReplaceCommand));
                            break;
                        case Commands.Set:
                            var setCommand = (SetNewCommand*) commandPointer;
                            _manager.SetComponent(
                                setCommand->Entity.DataIndex == -1 ? lastEntity : setCommand->Entity,
                                setCommand->ComponentTypeIndex,
                                (byte*) setCommand + sizeof(SetNewCommand));
                            break;
                        case Commands.Remove:
                            var removeCommand = (RemoveCommand*) commandPointer;
                            _manager.RemoveComponent(removeCommand->Entity, removeCommand->ComponentTypeIndex);
                            break;
                        case Commands.Delete:
                            var deleteCommand = (DeleteCommand*) commandPointer;
                            _manager.DeleteEntity(deleteCommand->Entity);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    offset += commandPointer->Size;
                }

                var currentPointer = (IntPtr) chunk;
                chunk = chunk->Next;
                Marshal.FreeHGlobal(currentPointer);
            }
        }

        private unsafe ref T GetPointer<T>(int size)
        {
            var commandSize = Unsafe.SizeOf<T>();
            if (_last == null)
            {
                _first = _last = (CommandChunk*) Marshal.AllocHGlobal(sizeof(CommandChunk) + 1024);
                _first->Size = 1024;
                _first->Used = 0;
                _first->Previous = null;
                _first->Next = null;
            }

            if (_last->Size < _last->Used + commandSize + size)
            {
                var next = (CommandChunk*) Marshal.AllocHGlobal(sizeof(CommandChunk) + 1024);
                next->Size = 1024;
                next->Used = 0;
                _last->Next = next;
                next->Previous = _last;
                _last = next;
                _last->Next = null;
            }

            var used = _last->Used;
            _last->Used += commandSize + size;

            return ref Unsafe.AsRef<T>((byte*) _last + sizeof(CommandChunk) + used);
        }

        internal unsafe struct CommandChunk
        {
            public CommandChunk* Next;
            public CommandChunk* Previous;

            public int Used;
            public int Size;
        }

        private enum Commands
        {
            Create,
            Add,
            Replace,
            Set,
            Remove,
            Delete
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CommandHeader
        {
            public Commands CommandType;
            public int Size;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CreateCommand
        {
            public CommandHeader Header;
            public Blueprint Blueprint;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SetNewCommand
        {
            public CommandHeader Header;
            public Entity Entity;
            public int ComponentTypeIndex;
            public int ComponentSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AddCommand
        {
            public CommandHeader Header;
            public Entity Entity;
            public int ComponentTypeIndex;
            public int ComponentSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ReplaceCommand
        {
            public CommandHeader Header;
            public Entity Entity;
            public int ComponentTypeIndex;
            public int ComponentSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RemoveCommand
        {
            public CommandHeader Header;
            public Entity Entity;
            public int ComponentTypeIndex;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DeleteCommand
        {
            public CommandHeader Header;
            public Entity Entity;
        }
    }

    internal sealed class EntityCommandBuffer<T> : IEntityCommandBuffer<T>, INewEntityCommandBuffer<T>
    {
        private readonly EntityCommandBuffer _commandBuffer;

        public EntityCommandBuffer(EntityManager manager)
        {
            _commandBuffer = new EntityCommandBuffer(manager);
        }

        public INewEntityCommandBuffer<T> CreateEntity(in Blueprint blueprint)
        {
            _commandBuffer.CreateEntity(blueprint);
            return this;
        }

        public INewEntityCommandBuffer<T> CreateEntity<TDefinition>(in Blueprint<TDefinition> blueprint)
        {
            _commandBuffer.CreateEntity(blueprint.RealBlueprint);
            return this;
        }

        public unsafe INewEntityCommandBuffer<T> CreateEntity()
        {
            _commandBuffer.CreateEntity(new Blueprint(null));
            return this;
        }

        public IEntityCommandBuffer<T> AddComponent<T1>(in Entity entity) where T1 : unmanaged, IComponent
        {
            _commandBuffer.AddComponent<T1>(entity, default);
            return this;
        }

        public IEntityCommandBuffer<T> AddComponent<T1>(in Entity entity, in T1 component)
            where T1 : unmanaged, IComponent
        {
            _commandBuffer.AddComponent(entity, component);
            return this;
        }

        public IEntityCommandBuffer<T> ReplaceComponent<T1>(in Entity entity, in T1 component)
            where T1 : unmanaged, IComponent
        {
            _commandBuffer.ReplaceComponent(entity, component);
            return this;
        }

        public IEntityCommandBuffer<T> SetComponent<T1>(in Entity entity, in T1 component)
            where T1 : unmanaged, IComponent
        {
            _commandBuffer.SetComponent(entity, component);
            return this;
        }

        public IEntityCommandBuffer<T> RemoveComponent<T1>(in Entity entity) where T1 : unmanaged, IComponent
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

        public INewEntityCommandBuffer<T> SetComponent<T1>(in T1 component) where T1 : unmanaged, IComponent
        {
            _commandBuffer.SetComponent(new Entity(-1, -1), component);
            return this;
        }

        public IEntityCommandBuffer<T> Finish()
        {
            return this;
        }
    }
}