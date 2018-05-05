namespace Mugen.Abstraction.CommandBuffers
{
    using CommandBuffer;

    internal interface IEntityCommandBuffer
    {
        void CreateEntity(Blueprint blueprint);
        void CreateEntity();

        void AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        void ReplaceComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        void SetComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        void RemoveComponent<T>(in Entity entity) where T : struct, IComponent;

        void DeleteEntity(in Entity entity);

        void Playback();
    }

    public interface IEntityCommandBuffer<TSystem>
    {
        INewEntityCommandBuffer<TSystem> CreateEntity(Blueprint blueprint);
        INewEntityCommandBuffer<TSystem> CreateEntity();

        IEntityCommandBuffer<TSystem> AddComponent<T>(in Entity entity) where T : struct, IComponent;
        IEntityCommandBuffer<TSystem> AddComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> ReplaceComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> SetComponent<T>(in Entity entity, in T component) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> RemoveComponent<T>(in Entity entity) where T : struct, IComponent;

        IEntityCommandBuffer<TSystem> DeleteEntity(in Entity entity);

        void Playback();
    }
}