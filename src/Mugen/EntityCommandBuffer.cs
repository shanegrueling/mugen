namespace Mugen
{
    using System;
    using System.Collections.Generic;

    internal class EntityCommandBuffer : IEntityCommandBuffer
    {
        private readonly EntityManager _manager;
        private readonly List<IEntityCommand> _commandList;

        public EntityCommandBuffer(EntityManager manager)
        {
            _manager = manager;
            _commandList = new List<IEntityCommand>();
        }

        public INewEntityCommandBuffer CreateEntity(Blueprint blueprint)
        {
            var necb = new NewEntityCommandBuffer(this, blueprint);
            _commandList.Add(necb);

            return necb;
        }

        public IEntityCommandBuffer AddComponent<T>(Entity entity)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer AddComponent<T>(Entity entity, T component)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer ReplaceComponent<T>(Entity entity, T component)
        {
            throw new NotImplementedException();
        }

        public IEntityCommandBuffer RemoveComponent<T>(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Playback()
        {
            for (var i = 0; i < _commandList.Count; ++i)
            {
                _commandList[i].Invoke();
            }

            _commandList.Clear();
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
            private readonly List<INewEntityCommand> _commandList;

            public NewEntityCommandBuffer(EntityCommandBuffer parent, Blueprint blueprint)
            {
                _parent = parent;
                _blueprint = blueprint;
                _commandList = new List<INewEntityCommand>(blueprint.Types.Length + 1)
                {
                    new CreateEntity(this)
                };
            }

            public INewEntityCommandBuffer ReplaceComponent<T>(T component) where T : struct, IComponent
            {
                _commandList.Add(new ReplaceEntity<T>(this, component));

                return this;
            }

            public IEntityCommandBuffer Finish()
            {
                return _parent;
            }

            public void Invoke()
            {
                for (var i = 0; i < _commandList.Count; ++i)
                {
                    _commandList[i].Invoke();
                }
            }

            private interface INewEntityCommand
            {
                void Invoke();
            }

            private class CreateEntity : INewEntityCommand
            {
                private readonly NewEntityCommandBuffer _buffer;

                public CreateEntity(NewEntityCommandBuffer buffer)
                {
                    _buffer = buffer;
                }

                public void Invoke()
                {
                    _buffer._entity = _buffer._parent._manager.CreateEntity(_buffer._blueprint);
                }
            }

            private class ReplaceEntity<T> : INewEntityCommand where T : struct, IComponent
            {
                private readonly NewEntityCommandBuffer _buffer;
                private readonly T _comp;

                public ReplaceEntity(NewEntityCommandBuffer buffer, T component)
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