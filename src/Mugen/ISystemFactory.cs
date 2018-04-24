namespace Mugen
{
    public interface ISystemFactory
    {
        ISystem Create<T>(World world);
    }
}