namespace Mugen
{
    public interface ISystem
    {
        bool HasUpdateMethod { get; }
        bool IsAsync { get; }
    }
}