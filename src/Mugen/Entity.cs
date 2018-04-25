namespace Mugen
{
    using System;
    using System.Collections.Generic;

    public struct Entity : IEquatable<Entity>
    {
        private static int _counter;
        private readonly int _id;

        private Entity(int id)
        {
            _id = id;
        }

        public static Entity Create() => new Entity(_counter++);

        public bool Equals(Entity other)
        {
            return _id == other._id;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Entity && Equals((Entity)obj);
        }

        public override int GetHashCode()
        {
            return _id;
        }

        public static bool operator ==(Entity left, Entity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Entity left, Entity right)
        {
            return !left.Equals(right);
        }
    }
    

    public class EntityEqualityComparer : IEqualityComparer<Entity>
    {
        public bool Equals(Entity x, Entity y) => x.Equals(y);

        public int GetHashCode(Entity obj) => obj.GetHashCode();
    }
}