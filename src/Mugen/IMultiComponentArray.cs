namespace Mugen
{
    internal interface IMultiComponentArray
    {
        int Length { get; }
        void Add(object componentArray);
    }
}