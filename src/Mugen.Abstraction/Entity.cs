

namespace Mugen.Abstraction
{
    using System;

    public struct Entity : IEquatable<Entity>
    {
        internal readonly int DataIndex;
        internal readonly int Version;

        public Entity(int dataIndex, int version)
        {
            DataIndex = dataIndex;
            Version = version;
        }

        public bool Equals(Entity other)
        {
            return DataIndex == other.DataIndex && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is Entity entity && Equals(entity);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DataIndex * 397) ^ Version;
            }
        }
    }
}
