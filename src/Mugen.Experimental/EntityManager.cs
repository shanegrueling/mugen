namespace Mugen.Experimental
{
    using System;
    using System.Collections.Generic;
    using Abstraction;
    using Abstraction.CommandBuffers;

    internal class EntityManager : IEntityManager, IDisposable
    {
        private readonly BlueprintManager _blueprintManager;
        private readonly ComponentTypeComparer _comparer;
        private readonly EntityDataManager _entityDataManager;

        private readonly List<ComponentMatcher> _componentMatchers;

        private readonly Dictionary<Type, object> _dictionary;

        public EntityManager()
        {
            _blueprintManager = new BlueprintManager();
            _entityDataManager = new EntityDataManager(_blueprintManager);
            _comparer = new ComponentTypeComparer();
            _componentMatchers = new List<ComponentMatcher>();
            _dictionary = new Dictionary<Type, object>();
        }

        public IComponentMatcher GetMatcher(params ComponentMatcherTypes[] matcherTypes)
        {
            var matcher = new ComponentMatcher(matcherTypes);

            _blueprintManager.CheckBlueprintsForMatcher(matcher);

            _componentMatchers.Add(matcher);
            return matcher;
        }

        public Blueprint CreateBlueprint(params ComponentType[] types)
        {
            InvalidateMatchers();
            Array.Sort(types, _comparer);
            return _blueprintManager.GetOrCreateBlueprint(types);
        }

        public bool Exist(Entity entity)
        {
            return _entityDataManager.Exists(entity);
        }

        public bool HasComponent<T>(Entity entity) where T : struct, IComponent
        {
            throw new NotImplementedException();
        }

        public bool HasComponent(Entity entity, ComponentType T)
        {
            throw new NotImplementedException();
        }

        public ref T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ref _entityDataManager.GetComponent<T>(entity, TypeManager.GetIndex<T>());
        }

        public Entity CreateEntity(Blueprint blueprint)
        {
            InvalidateMatchers();
            return _entityDataManager.CreateEntity(blueprint);
        }

        public Entity CreateEntity()
        {
            InvalidateMatchers();
            return _entityDataManager.CreateEntity();
        }

        public void AddComponent<T>(in Entity entity) where T : struct, IComponent
        {
            InvalidateMatchers();
            _entityDataManager.AddComponent<T>(entity);
        }

        public void AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            InvalidateMatchers();

            _entityDataManager.AddComponent(entity, component);
        }

        public unsafe void AddComponent(in Entity entity, in int componentTypeIndex, byte* pointer)
        {
            
        }

        public unsafe void SetComponent(in Entity entity, in int componentTypeIndex, byte* pointer)
        {
            _entityDataManager.SetComponent(entity, componentTypeIndex, pointer);
        }

        public void ReplaceComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            _entityDataManager.GetComponent<T>(entity, TypeManager.GetIndex<T>()) = component;
        }

        internal void RemoveComponent(Entity entity, int componentTypeIndex)
        {
            throw new NotImplementedException();
        }

        public void SetComponent<T>(in Entity entity, in T component) where T : struct, IComponent
        {
            throw new System.NotImplementedException();
        }

        public void RemoveComponent<T>(in Entity entity)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteEntity(in Entity entity)
        {
            InvalidateMatchers();

            _entityDataManager.DeleteEntity(entity);
        }

        public IEntityCommandBuffer<TSystem> CreateCommandBuffer<TSystem>()
        {
            if(_dictionary.TryGetValue(typeof(TSystem), out var commandBuffer))
            {
                return (IEntityCommandBuffer<TSystem>)commandBuffer;
            }
            _dictionary[typeof(TSystem)] = new EntityCommandBuffer<TSystem>(this);
            return (IEntityCommandBuffer<TSystem>)_dictionary[typeof(TSystem)];
        }

        public IEntityCommandBuffer<TSystem> GetCommandBuffer<TSystem>()
        {
            if(_dictionary.TryGetValue(typeof(TSystem), out var commandBuffer))
            {
                return (IEntityCommandBuffer<TSystem>)commandBuffer;
            }

            return null;
        }

        private void InvalidateMatchers()
        {
            foreach (var matcher in _componentMatchers)
            {
                matcher.Invalidate();
            }
        }

        public void Dispose()
        {
            _entityDataManager.Dispose();
            _blueprintManager.Dispose();
            foreach (var matcher in _componentMatchers)
            {
                matcher.Dispose();
            }
        }

        private class ComponentTypeComparer : IComparer<ComponentType>
        {
            public int Compare(ComponentType x, ComponentType y)
            {
                return y.DataIndex - x.DataIndex;
            }
        }
    }
}