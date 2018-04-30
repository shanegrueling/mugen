namespace Mugen
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal unsafe class EntityManager : IEntityManager
    {
        internal int Version;
        private readonly List<Blueprint> _blueprints;
        private readonly List<BlueprintContainer> _blueprintContainers;

        private readonly List<ComponentMatcher> _matcher;

        private struct EntityInfo
        {
            public int BlueprintIndex;
            public int Version;
        }

        private EntityInfo* _entityInfos;
        private int _size;
        private int _entityCount;

        private readonly Queue<int> _freeEntities;


        public EntityManager()
        {
            _blueprints = new List<Blueprint>();
            _blueprintContainers = new List<BlueprintContainer>();
            _matcher = new List<ComponentMatcher>();

            _size = 1024;
            _entityInfos = (EntityInfo*) Marshal.AllocHGlobal(_size * sizeof(EntityInfo));
            _freeEntities = new Queue<int>();
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
            
            entity.EntityInfoIndex = _freeEntities.Count > 0 ? _freeEntities.Dequeue() : _entityCount++;

            if(_entityCount > _size)
            {
                Resize();
            }

            // ReSharper disable once PossibleNullReferenceException
            ref var entityInfo = ref _entityInfos[entity.EntityInfoIndex];
            entityInfo.BlueprintIndex = blueprint.Index;
            entityInfo.Version += 1;

            _blueprintContainers[blueprint.Index].AddEntity(entity);

            ++Version;

            return entity;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Resize()
        {
            _size *= 2;

            _entityInfos = (EntityInfo*)Marshal.ReAllocHGlobal((IntPtr) _entityInfos, new IntPtr(_size * sizeof(EntityInfo)));
        }

        public ref T GetComponent<T>(in Entity entity) where T : struct, IComponent
        {
            return ref _blueprintContainers[_entityInfos[entity.EntityInfoIndex].BlueprintIndex].GetComponent<T>(entity);
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