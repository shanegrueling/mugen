namespace Mugen.Abstraction
{
    using System;

    public struct ComponentMatcherTypes
    {
        internal readonly int DataIndex;
        internal readonly ComponentMatcherTypesAccess Access;

        public ComponentMatcherTypes(Type t, ComponentMatcherTypesAccess access)
        {
            Access = access;
            DataIndex = TypeManager.GetIndex(t);
        }

        public static implicit operator ComponentMatcherTypes(Type t)
        {
            return new ComponentMatcherTypes(t, ComponentMatcherTypesAccess.ReadWrite);
        }
    }
}