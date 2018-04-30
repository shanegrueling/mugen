namespace Mugen.Abstraction.CommandBuffers
{
    using CommandBuffer;

    public interface IEntityCommandBuffer<TSystem>
    {
        INewEntityCommandBuffer<TSystem> CreateEntity(Blueprint blueprint);
        INewEntityCommandBuffer<TSystem> CreateEntity();

        IEntityCommandBuffer<TSystem> AddComponent<T>(in Entity entity);
        IEntityCommandBuffer<TSystem> AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> ReplaceComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> SetComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> RemoveComponent<T>(in Entity entity) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> DeleteEntity(in Entity entity);

        void Playback();
    }
}