namespace Mugen
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;

    internal class BlueprintContainer
    {
        private readonly Blueprint _blueprint;
        private Entity[] _entity;
        private int _entityCount;
        private readonly Dictionary<Entity, int> _entityToIndex;
        private readonly Dictionary<Type, IComponentArray> _componentArrays;

        public BlueprintContainer(Blueprint blueprint)
        {
            _blueprint = blueprint;
            _componentArrays = new Dictionary<Type, IComponentArray>(_blueprint.Types.Length);
            _entity = ArrayPool<Entity>.Shared.Rent(1024);
            _entityToIndex = new Dictionary<Entity, int>(new EntityEqualityComparer());

            foreach(var blueprintType in _blueprint.Types)
            {
                _componentArrays[blueprintType] = ComponentArrayFactory.CreateNew(blueprintType, 1024);
            }
        }

        public object GetComponentArray(Type requiredType)
        {
            return _componentArrays[requiredType];
        }

        public IComponentArray<T> GetComponentArray<T>() where T : struct, IComponent
        {
            return (IComponentArray<T>)GetComponentArray(typeof(T));
        }

        private void EnsureCapacity(int count)
        {
            if(count < _entity.Length) return;

            var newItems = ArrayPool<Entity>.Shared.Rent(_entityCount * 2);
            Array.Copy(_entity, 0, newItems, 0, _entityCount);
            ArrayPool<Entity>.Shared.Return(newItems);

            _entity = newItems;
        }

        public void AddEntity(Entity entity)
        {
            _entityToIndex.Add(entity, _entityCount);
            EnsureCapacity(_entityCount + 1);
            _entity[_entityCount++] = entity;
            foreach(var componentArray in _componentArrays)
            {
                componentArray.Value.CreateNew();
            }
        }

        public ref T GetComponent<T>(in Entity entity) where T : struct, IComponent => ref GetComponentArray<T>()[_entityToIndex[entity]];
    }
}