namespace Mugen
{
    using System;

    public interface IEntityManager
    {
        IComponentMatcher GetMatcher(params Type[] types);
        Blueprint CreateBlueprint(params Type[] type);
        IEntityCommandBuffer CreateBuffer();

        Entity CreateEntity(Blueprint blueprint);
        ref T GetComponent<T>(in Entity entity) where T : struct, IComponent;
    }

    public interface IEntityCommandBuffer
    {
        INewEntityCommandBuffer CreateEntity(Blueprint blueprint);

        IEntityCommandBuffer AddComponent<T>(in Entity entity);
        IEntityCommandBuffer AddComponent<T>(in Entity entity, in T component);

        IEntityCommandBuffer ReplaceComponent<T>(in Entity entity, in T component);

        IEntityCommandBuffer SetComponent<T>(in Entity entity, in T component);

        IEntityCommandBuffer RemoveComponent<T>(in Entity entity);

        void Playback();
    }

    public interface INewEntityCommandBuffer
    {
        INewEntityCommandBuffer ReplaceComponent<T>(in T component) where T : struct, IComponent;
        IEntityCommandBuffer Finish();
    }
}