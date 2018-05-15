namespace Mugen.Abstraction
{
    using CommandBuffers;

    public interface IEntityManager
    {
        IComponentMatcher GetMatcher(params ComponentMatcherTypes[] matcher);

        Blueprint CreateBlueprint(params ComponentType[] types);

        bool Exist(Entity entity);
        bool HasComponent<T>(Entity entity) where T : unmanaged, IComponent;
        bool HasComponent(Entity entity, ComponentType T);
        ref T GetComponent<T>(Entity entity) where T : unmanaged, IComponent;

        Entity CreateEntity(Blueprint blueprint);
        Entity CreateEntity<TDefinition>(Blueprint<TDefinition> blueprint);
        Entity CreateEntity();

        void AddComponent<T>(in Entity entity) where T : unmanaged, IComponent;
        void AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;
        void ReplaceComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;
        void SetComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;
        void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent;
        void DeleteEntity(in Entity entity);

        IEntityCommandBuffer<TSystem> CreateCommandBuffer<TSystem>();
        IEntityCommandBuffer<TSystem> GetCommandBuffer<TSystem>();
    }
}