namespace Mugen
{
    using System;
    using System.Collections.Generic;
    using Abstraction;
    using Abstraction.CommandBuffers;

    internal class EntityManager : IEntityManager, IDisposable
    {
        private readonly BlueprintManager _blueprintManager;
        private readonly ComponentTypeComparer _comparer;

        private readonly List<ComponentMatcher> _componentMatchers;

        private readonly Dictionary<Type, object> _dictionary;
        private readonly EntityDataManager _entityDataManager;

        public EntityManager()
        {
            _blueprintManager = new BlueprintManager();
            _entityDataManager = new EntityDataManager(_blueprintManager);
            _comparer = new ComponentTypeComparer();
            _componentMatchers = new List<ComponentMatcher>();
            _dictionary = new Dictionary<Type, object>();
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

        public bool HasComponent<T>(Entity entity) where T : unmanaged, IComponent
        {
            return _entityDataManager.HasComponent(entity, TypeManager.GetIndex<T>());
        }

        public bool HasComponent(Entity entity, ComponentType T)
        {
            return _entityDataManager.HasComponent(entity, T.DataIndex);
        }

        public ref T GetComponent<T>(Entity entity) where T : unmanaged, IComponent
        {
            return ref _entityDataManager.GetComponent<T>(entity, TypeManager.GetIndex<T>());
        }

        public Entity CreateEntity(Blueprint blueprint)
        {
            InvalidateMatchers();

            return _entityDataManager.CreateEntity(blueprint);
        }

        public Entity CreateEntity<TDefinition>(Blueprint<TDefinition> blueprint)
        {
            InvalidateMatchers();

            return _entityDataManager.CreateEntity(blueprint.RealBlueprint);
        }

        public Entity CreateEntity()
        {
            InvalidateMatchers();

            return _entityDataManager.CreateEntity();
        }

        public void AddComponent<T>(in Entity entity) where T : unmanaged, IComponent
        {
            InvalidateMatchers();

            _entityDataManager.AddComponent<T>(entity);
        }

        public void AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            InvalidateMatchers();

            _entityDataManager.AddComponent(entity, component);
        }

        public void ReplaceComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            _entityDataManager.GetComponent<T>(entity, TypeManager.GetIndex<T>()) = component;
        }

        public void SetComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent
        {
            InvalidateMatchers();

            _entityDataManager.SetComponent(entity, TypeManager.GetIndex<T>(), component);
        }

        public void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent
        {
            InvalidateMatchers();

            _entityDataManager.RemoveComponent(entity, TypeManager.GetIndex<T>());
        }

        public void DeleteEntity(in Entity entity)
        {
            InvalidateMatchers();

            _entityDataManager.DeleteEntity(entity);
        }

        public IEntityCommandBuffer<TSystem> CreateCommandBuffer<TSystem>()
        {
            if (_dictionary.TryGetValue(typeof(TSystem), out var commandBuffer))
            {
                return (IEntityCommandBuffer<TSystem>) commandBuffer;
            }

            _dictionary[typeof(TSystem)] = new EntityCommandBuffer<TSystem>(this);
            return (IEntityCommandBuffer<TSystem>) _dictionary[typeof(TSystem)];
        }

        public IEntityCommandBuffer<TSystem> GetCommandBuffer<TSystem>()
        {
            if (_dictionary.TryGetValue(typeof(TSystem), out var commandBuffer))
            {
                return (IEntityCommandBuffer<TSystem>) commandBuffer;
            }

            return null;
        }

        public unsafe void AddComponent(in Entity entity, in int componentTypeIndex, byte* pointer)
        {
            InvalidateMatchers();

            _entityDataManager.AddComponent(entity, componentTypeIndex, pointer);
        }

        public unsafe void SetComponent(in Entity entity, in int componentTypeIndex, byte* pointer)
        {
            InvalidateMatchers();

            _entityDataManager.SetComponent(entity, componentTypeIndex, pointer);
        }

        internal unsafe void ReplaceComponent(Entity entity, int componentTypeIndex, byte* pointer)
        {
            _entityDataManager.SetComponent(entity, componentTypeIndex, pointer);
        }

        internal void RemoveComponent(Entity entity, int componentTypeIndex)
        {
            InvalidateMatchers();

            _entityDataManager.RemoveComponent(entity, componentTypeIndex);
        }

        private void InvalidateMatchers()
        {
            foreach (var matcher in _componentMatchers)
            {
                matcher.Invalidate();
            }
        }

        private class ComponentTypeComparer : IComparer<ComponentType>
        {
            public int Compare(ComponentType x, ComponentType y)
            {
                return x.DataIndex - y.DataIndex;
            }
        }
    }
}