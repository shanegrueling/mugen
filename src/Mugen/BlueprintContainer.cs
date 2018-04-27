namespace Mugen
{
    using System;
    using System.Buffers;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class BlueprintContainer
    {
        private readonly Blueprint _blueprint;
        private Entity[] _entity;
        private int _entityCount;
        private readonly Dictionary<Entity, int> _entityToIndex;
        private readonly IComponentArray[] _componentArrays;

        public BlueprintContainer(Blueprint blueprint)
        {
            _blueprint = blueprint;
            _componentArrays = new IComponentArray[_blueprint.Types.Length];
            _entity = ArrayPool<Entity>.Shared.Rent(1024);
            _entityToIndex = new Dictionary<Entity, int>(new EntityEqualityComparer());

            for(var i = 0; i < _blueprint.Types.Length; ++i)
            {
                _componentArrays[i] = ComponentArrayFactory.CreateNew(blueprint.Types[i], 1024);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetComponentArray(Type requiredType)
        {
            for(var i = 0; i < _blueprint.Types.Length; i++)
            {
                if(_blueprint.Types[i] == requiredType) return _componentArrays[i];
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ComponentArrayPool<T> GetComponentArray<T>() where T : struct, IComponent
        {
            return (ComponentArrayPool<T>) GetComponentArray(typeof(T));
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

            for(var i = 0; i < _componentArrays.Length; ++i)
            {
                _componentArrays[i].CreateNew();
            }
        }

        public ref T GetComponent<T>(in Entity entity) where T : struct, IComponent => ref GetComponentArray<T>()[_entityToIndex[entity]];
    }
}