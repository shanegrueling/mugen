namespace Mugen
{
    internal interface IMultiComponentArray
    {
        int Count { get; }
        void Add(object componentArray);
    }
}