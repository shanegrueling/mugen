namespace Mugen
{
    using System;

    public interface IEntityManager
    {
        IComponentMatcher GetMatcher(params Type[] types);
        Blueprint CreateBlueprint(params Type[] type);
        IEntityCommandBuffer CreateBuffer();
    }

    public interface IEntityCommandBuffer
    {
        INewEntityCommandBuffer CreateEntity(Blueprint blueprint);

        IEntityCommandBuffer AddComponent<T>(Entity entity);
        IEntityCommandBuffer AddComponent<T>(Entity entity, T component);

        IEntityCommandBuffer ReplaceComponent<T>(Entity entity, T component);

        IEntityCommandBuffer RemoveComponent<T>(Entity entity);

        void Playback();
    }

    public interface INewEntityCommandBuffer
    {
        INewEntityCommandBuffer ReplaceComponent<T>(T component) where T : struct, IComponent;
        IEntityCommandBuffer Finish();
    }
}