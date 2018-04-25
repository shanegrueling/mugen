namespace Mugen
{
    using System;
    using System.Collections.Generic;
    using StructList;

    internal class BlueprintContainer
    {
        private readonly Blueprint _blueprint;
        private readonly StructList<Entity> _entity;
        private readonly Dictionary<Entity, int> _entityToIndex;
        private readonly Dictionary<Type, IComponentArray> _componentArrays;

        public BlueprintContainer(Blueprint blueprint)
        {
            _blueprint = blueprint;
            _componentArrays = new Dictionary<Type, IComponentArray>(_blueprint.Types.Length);
            _entity = new StructList<Entity>(1000);
            _entityToIndex = new Dictionary<Entity, int>(new EntityEqualityComparer());

            foreach(var blueprintType in _blueprint.Types)
            {
                _componentArrays[blueprintType] = ComponentArrayFactory.CreateNew(blueprintType, 1000);
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

        public void AddEntity(Entity entity)
        {
            _entityToIndex.Add(entity, _entity.Count);
            _entity.Add(entity);
            foreach(var componentArray in _componentArrays)
            {
                componentArray.Value.CreateNew();
            }
        }

        public ref T GetComponent<T>(Entity entity) where T : struct, IComponent => ref GetComponentArray<T>()[_entityToIndex[entity]];
    }
}