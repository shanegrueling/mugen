namespace Mugen.Abstraction
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public class GeneratedComponentSystemAttribute : Attribute
    {
        private readonly Type _baseSystem;

        public GeneratedComponentSystemAttribute(Type baseSystem)
        {
            _baseSystem = baseSystem;
        }
    }
}