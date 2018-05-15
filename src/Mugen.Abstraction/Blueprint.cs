namespace Mugen.Abstraction
{
    using System;
    using System.Runtime.InteropServices;
    using BlueprintManager;

    public unsafe struct Blueprint : IEquatable<Blueprint>
    {
        internal readonly BlueprintData* BlueprintData;

        internal Blueprint(BlueprintData* blueprintData)
        {
            BlueprintData = blueprintData;
        }

        public bool Equals(Blueprint other)
        {
            return BlueprintData == other.BlueprintData;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is Blueprint blueprint && Equals(blueprint);
        }

        public override int GetHashCode()
        {
            return unchecked((int) (long) BlueprintData);
        }
    }

    public readonly struct Blueprint<TDefinition>
    {
        internal readonly Blueprint RealBlueprint;

        public Blueprint(Blueprint blueprint)
        {
            RealBlueprint = blueprint;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct BlueprintEntityChunk
    {
        public BlueprintEntityChunk* NextChunk;
        public BlueprintEntityChunk* ChunkWithSpace;

        public int Capacity;
        public int EntityCount;

        public BlueprintData* Blueprint;

        public fixed byte Buffer[1];

        public BlueprintEntityChunk(BlueprintData* blueprint, int capacity)
        {
            Blueprint = blueprint;
            Capacity = capacity;
            EntityCount = 0;
            NextChunk = null;
            ChunkWithSpace = null;
        }
    }
}