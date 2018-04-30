namespace Mugen.Abstraction
{
    using System;

    public struct ComponentType
    {
        internal readonly int DataIndex;

        public ComponentType(Type t)
        {
            DataIndex = TypeManager.GetIndex(t);
        }

        public static implicit operator ComponentType(Type t)
        {
            return new ComponentType(t);
        }
    }
}