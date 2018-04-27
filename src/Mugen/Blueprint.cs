namespace Mugen
{
    using System;
    using System.Linq;

    public sealed class Blueprint
    {
        internal Type[] Types { get; }
        internal int Index { get; set; }

        internal Blueprint(params Type[] types)
        {
            Types = types;
        }

        public bool Fits(Type[] type) => Types.Length == type.Length && Types.All(type.Contains);
    }
}