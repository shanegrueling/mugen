namespace Mugen
{
    using System;
    using System.Collections.Generic;

    internal class EntityCommandBuffer : IEntityCommandBuffer
    {
        private readonly EntityManager _manager;
        private readonly List<Action<EntityManager>> _commandList;

        public EntityCommandBuffer(EntityManager manager)
        {
            _manager = manager;
            _commandList = new List<Action<EntityManager>>();
        }

        public INewEntityCommandBuffer CreateEntity(Blueprint blueprint)
        {
            var necb = new NewEntityCommandBuffer(this, blueprint);
            _commandList.Add(manager => necb.Invoke(manager));

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
            foreach(var command in _commandList)
            {
                command(_manager);
            }

            _commandList.Clear();
        }

        private class NewEntityCommandBuffer : INewEntityCommandBuffer
        {
            private readonly EntityCommandBuffer _parent;
            private Entity _entity;
            private Blueprint _blueprint;
            private List<Action<EntityManager>> _commandList;

            public NewEntityCommandBuffer(EntityCommandBuffer parent, Blueprint blueprint)
            {
                _parent = parent;
                _blueprint = blueprint;
                _commandList = new List<Action<EntityManager>>()
                {
                    (manager => _entity = manager.CreateEntity(_blueprint))
                };
            }

            public INewEntityCommandBuffer ReplaceComponent<T>(T component) where T : struct, IComponent
            {
                _commandList.Add(manager =>
                {
                    ref var comp = ref manager.GetComponent<T>(_entity);
                    comp = component;
                });

                return this;
            }

            public IEntityCommandBuffer Finish()
            {
                return _parent;
            }

            public void Invoke(EntityManager manager)
            {
                foreach(var command in _commandList)
                {
                    command(manager);
                }
            }
        }
    }
}