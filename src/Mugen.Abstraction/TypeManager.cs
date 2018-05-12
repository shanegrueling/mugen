using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Mugen.Experimental")]
namespace Mugen.Abstraction
{
    using System;
    using System.Runtime.InteropServices;

    internal static class TypeManager
    {
        private struct StaticTypeLookup<T>
        {
            public static int TypeIndex = -1;
        }

        public struct ComponentType
        {
            public readonly Type Type;
            public readonly int Size;

            public ComponentType(Type type)
            {
                Type = type;
                Size = Marshal.SizeOf(type);
            }
        }

        private static readonly ComponentType[] ComponentTypes = new ComponentType[1024];
        private static int _typeCount = 1;

        public static int GetIndex(Type type)
        {
            var index = FindTypeIndex(type);
            return index == -1 ? CreateNewComponentType(type) : index;
        }

        public static ref ComponentType GetComponentType(int index)
        {
            return ref ComponentTypes[index];
        }

        private static int FindTypeIndex(Type type)
        {
            for (var i = 0; i < _typeCount;++i)
            {
                if (ComponentTypes[i].Type == type) return i;
            }

            return -1;
        }

        private static int CreateNewComponentType(Type type)
        {
            ComponentTypes[_typeCount] = new ComponentType(type);
            return _typeCount++;
        }

        internal static int GetIndex<T>() where T : unmanaged, IComponent
        {
            if (StaticTypeLookup<T>.TypeIndex == -1)
            {
                StaticTypeLookup<T>.TypeIndex = GetIndex(typeof(T));
            }

            return StaticTypeLookup<T>.TypeIndex;
        }
    }
}