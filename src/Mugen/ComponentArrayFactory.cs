﻿namespace Mugen
{
    using System;

    internal static class ComponentArrayFactory
    {
        private static readonly Type ComponentArrayType = typeof(ComponentArrayPool<>);
        public static IComponentArray CreateNew(Type t, int capacity)
        {
            var genericType = ComponentArrayType.MakeGenericType(t);
            return (IComponentArray)Activator.CreateInstance(genericType, capacity);
        }
    }
}