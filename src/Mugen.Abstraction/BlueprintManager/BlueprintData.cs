namespace Mugen.Abstraction.BlueprintManager
{
    internal struct BlueprintComponentType
    {
        public readonly int TypeIndex;

        public BlueprintComponentType(int typeIndex)
        {
            TypeIndex = typeIndex;
        }

        public BlueprintComponentType(ComponentType type)
        {
            TypeIndex = type.DataIndex;
        }
    }

    internal unsafe struct BlueprintData
    {
        public BlueprintComponentType* ComponentTypes;
        public int ComponentTypesCount;

        public BlueprintEntityChunk* FirstChunk;
        public BlueprintEntityChunk* LastChunk;
        public BlueprintEntityChunk* ChunkWithSpace;

        public int* Offsets;
        public int* SizeOfs;

        public int EntityCount;

        public BlueprintData* PreviousBlueprintData;
    }
}