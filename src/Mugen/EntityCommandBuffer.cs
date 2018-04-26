using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Mugen.Test"), InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Mugen
{
    using System;

    internal class EntityCommandBuffer : IEntityCommandBuffer
    {
        private readonly IEntityManager _manager;
        private IEntityCommand[] _commandList;
        private int _commandCount;

        public EntityCommandBuffer(IEntityManager manager)
        {
            _manager = manager;
            _commandList = new IEntityCommand[16];
            _commandCount = 0;
        }

        private void AddCommand(IEntityCommand command)
        {
            if (_commandCount >= _commandList.Length)
            {
                var n = new IEntityCommand[_commandCount * 2];
                Array.Copy(_commandList, n, _commandCount);
                _commandList = n;
            }
            _commandList[_commandCount++] = command;
        }

        public INewEntityCommandBuffer CreateEntity(Blueprint blueprint)
        {
            var necb = new NewEntityCommandBuffer(this, blueprint);
            
            AddCommand(necb);

            return necb;
        }

        public IEntityCommandBuffer AddComponent<T>(in Entity entity)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer AddComponent<T>(in Entity entity, in T component)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer ReplaceComponent<T>(in Entity entity, in T component)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer SetComponent<T>(in Entity entity, in T component)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer RemoveComponent<T>(in Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Playback()
        {
            for (var i = 0; i < _commandCount; ++i)
            {
                _commandList[i].Invoke();
            }

            Array.Clear(_commandList, 0, _commandCount);
            _commandCount = 0;
        }

        private interface IEntityCommand
        {
            void Invoke();
        }

        private class NewEntityCommandBuffer : INewEntityCommandBuffer, IEntityCommand
        {
            private readonly EntityCommandBuffer _parent;
            private Entity _entity;
            private readonly Blueprint _blueprint;
            private readonly INewEntityCommand[] _commandList;
            private int _commandCount;

            public NewEntityCommandBuffer(EntityCommandBuffer parent, Blueprint blueprint)
            {
                _parent = parent;
                _blueprint = blueprint;
                _commandList = new INewEntityCommand[blueprint.Types.Length];
            }

            public INewEntityCommandBuffer ReplaceComponent<T>(in T component) where T : struct, IComponent
            {
                _commandList[_commandCount++] = new ReplaceEntity<T>(this, component);

                return this;
            }

            public IEntityCommandBuffer Finish() => _parent;

            public void Invoke()
            {
                _entity = _parent._manager.CreateEntity(_blueprint);
                for (var i = 0; i < _commandCount; ++i)
                {
                    _commandList[i].Invoke();
                }
            }

            private interface INewEntityCommand
            {
                void Invoke();
            }

            private class ReplaceEntity<T> : INewEntityCommand where T : struct, IComponent
            {
                private readonly NewEntityCommandBuffer _buffer;
                private readonly T _comp;

                public ReplaceEntity(NewEntityCommandBuffer buffer, in T component)
                {
                    _buffer = buffer;
                    _comp = component;
                }

                public void Invoke()
                {
                    ref var comp = ref _buffer._parent._manager.GetComponent<T>(_buffer._entity);
                    comp = _comp;
                }
            }
        }
    }
}