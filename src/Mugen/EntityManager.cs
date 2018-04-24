namespace Mugen
{
    using System;
    using System.Collections.Generic;

    internal class EntityManager : IEntityManager
    {
        internal int Version;
        private readonly Dictionary<Blueprint, BlueprintContainer> _blueprintContainers;
        private readonly List<ComponentMatcher> _matcher;
        private readonly Dictionary<Entity, Blueprint> _entityToBlueprint;

        public EntityManager()
        {
            _blueprintContainers = new Dictionary<Blueprint, BlueprintContainer>();
            _matcher = new List<ComponentMatcher>();
            _entityToBlueprint = new Dictionary<Entity, Blueprint>();
        }

        public IComponentMatcher GetMatcher(params Type[] types)
        {
            var matcher = new ComponentMatcher(this, types);
            foreach(var pair in _blueprintContainers)
            {
                if(matcher.DoesBlueprintMatch(pair.Key))
                {
                    matcher.AddBlueprintContainer(pair.Value);
                }
            }
            _matcher.Add(matcher);
            return matcher;
        }

        public Blueprint CreateBlueprint(params Type[] type)
        {
            foreach(var pair in _blueprintContainers)
            {
                if(pair.Key.Fits(type)) return pair.Key;
            }

            var blueprint = new Blueprint(type);
            var container = new BlueprintContainer(blueprint);
            _blueprintContainers[blueprint] = container;

            foreach(var matcher in _matcher)
            {
                if(matcher.DoesBlueprintMatch(blueprint))
                {
                    matcher.AddBlueprintContainer(container);
                }
            }

            return blueprint;
        }

        public IEntityCommandBuffer CreateBuffer()
        {
            return new EntityCommandBuffer(this);
        }

        public Entity CreateEntity(Blueprint blueprint)
        {
            var entity = Entity.Create();
            _entityToBlueprint[entity] = blueprint;
            _blueprintContainers[blueprint].AddEntity(entity);

            ++Version;

            return entity;
        }

        public ref T GetComponent<T>(Entity entity) where T : struct, IComponent
        {
            return ref _blueprintContainers[_entityToBlueprint[entity]].GetComponent<T>(entity);
        }
    }

    internal static class MultiComponentArray
    {
        private static readonly Type MultiContainerArrayType = typeof(MultiComponentArray<>);

        public static IMultiComponentArray CreateFrom(EntityManager manager, Type t)
        {
            var mcaGenericType = MultiContainerArrayType.MakeGenericType(t);
            return (IMultiComponentArray)Activator.CreateInstance(mcaGenericType, manager);
        }
    }

    internal interface IComponentArray
    {
        void CreateNew();
    }
}
