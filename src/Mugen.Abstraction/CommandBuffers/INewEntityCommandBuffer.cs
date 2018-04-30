namespace Mugen.Abstraction.CommandBuffer
{
    using CommandBuffers;

    public interface INewEntityCommandBuffer<TSystem>
    {
        INewEntityCommandBuffer<TSystem> SetComponent<T>(in T component) where T : struct, IComponent;
        IEntityCommandBuffer<TSystem> Finish();
    }
}