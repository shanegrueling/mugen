namespace Mugen.Abstraction.CommandBuffers
{
    public interface INewEntityCommandBuffer<TSystem>
    {
        INewEntityCommandBuffer<TSystem> SetComponent<T>(in T component) where T : unmanaged, IComponent;
        IEntityCommandBuffer<TSystem> Finish();
    }
}