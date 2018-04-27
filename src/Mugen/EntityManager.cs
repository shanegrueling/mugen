namespace Mugen
{
    using System;
    using System.Collections.Generic;

    internal class EntityManager : IEntityManager
    {
        internal int Version;
        private readonly List<Blueprint> _blueprints;
        private readonly List<BlueprintContainer> _blueprintContainers;

        private readonly List<ComponentMatcher> _matcher;

        public EntityManager()
        {
            _blueprints = new List<Blueprint>();
            _blueprintContainers = new List<BlueprintContainer>();
            _matcher = new List<ComponentMatcher>();
        }

        public IComponentMatcher GetMatcher(params Type[] types)
        {
            var matcher = new ComponentMatcher(this, types);
            for (var i = 0; i < _blueprints.Count; ++i)
            {
                if(matcher.DoesBlueprintMatch(_blueprints[i]))
                {
                    matcher.AddBlueprintContainer(_blueprintContainers[i]);
                }
            }
            _matcher.Add(matcher);
            return matcher;
        }

        public Blueprint CreateBlueprint(params Type[] type)
        {
            for (var i = 0; i < _blueprints.Count; ++i)
            {
                var blueprint = _blueprints[i];
                if (blueprint.Fits(type)) return blueprint;
            }

            var newBlueprint = new Blueprint(type) { Index = _blueprints.Count };
            var container = new BlueprintContainer(newBlueprint);
            _blueprints.Add(newBlueprint);
            _blueprintContainers.Add(container);

            foreach(var matcher in _matcher)
            {
                if(matcher.DoesBlueprintMatch(newBlueprint))
                {
                    matcher.AddBlueprintContainer(container);
                }
            }

            return newBlueprint;
        }

        public IEntityCommandBuffer CreateBuffer() => new EntityCommandBuffer(this);

        public Entity CreateEntity(Blueprint blueprint)
        {
            var entity = Entity.Create();
            entity.BlueprintIndex = blueprint.Index;
            _blueprintContainers[blueprint.Index].AddEntity(entity);

            ++Version;

            return entity;
        }

        public ref T GetComponent<T>(in Entity entity) where T : struct, IComponent
        {
            return ref _blueprintContainers[entity.BlueprintIndex].GetComponent<T>(entity);
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
