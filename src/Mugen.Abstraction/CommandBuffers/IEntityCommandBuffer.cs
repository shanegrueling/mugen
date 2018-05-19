namespace Mugen.Abstraction.CommandBuffers
{
    internal interface IEntityCommandBuffer
    {
        void CreateEntity(in Blueprint blueprint);
        void CreateEntity();

        void AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;

        void ReplaceComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;

        void SetComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;

        void RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent;

        void DeleteEntity(in Entity entity);

        void Playback();
    }

    public interface IEntityCommandBuffer<TSystem>
    {
        INewEntityCommandBuffer<TSystem> CreateEntity(in Blueprint blueprint);
        INewEntityCommandBuffer<TSystem> CreateEntity<TDefinition>(in Blueprint<TDefinition> blueprint);
        INewEntityCommandBuffer<TSystem> CreateEntity();

        IEntityCommandBuffer<TSystem> AddComponent<T>(in Entity entity) where T : unmanaged, IComponent;
        IEntityCommandBuffer<TSystem> AddComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;

        IEntityCommandBuffer<TSystem> ReplaceComponent<T>(in Entity entity, in T component)
            where T : unmanaged, IComponent;

        IEntityCommandBuffer<TSystem> SetComponent<T>(in Entity entity, in T component) where T : unmanaged, IComponent;

        IEntityCommandBuffer<TSystem> RemoveComponent<T>(in Entity entity) where T : unmanaged, IComponent;

        IEntityCommandBuffer<TSystem> DeleteEntity(in Entity entity);

        void Playback();
    }
}