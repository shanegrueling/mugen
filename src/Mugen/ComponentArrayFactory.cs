namespace Mugen
{
    using System;

    internal static class ComponentArrayFactory
    {
        private static readonly Type ComponentArrayType = typeof(ComponentArray<>);
        public static IComponentArray CreateNew(Type t)
        {
            var genericType = ComponentArrayType.MakeGenericType(t);
            return (IComponentArray)Activator.CreateInstance(genericType);
        }
    }
}