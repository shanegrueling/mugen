namespace Mugen
{
    using System;
    using System.Linq;

    internal class ComponentMatcher : IComponentMatcher
    {
        private readonly Type[] _requiredTypes;
        private readonly IMultiComponentArray[] _componentArrays;

        public int Length => _componentArrays[0].Count;

        public ComponentMatcher(EntityManager manager, Type[] types)
        {
            _requiredTypes = types;
            var i = 0;
            _componentArrays = new IMultiComponentArray[_requiredTypes.Length];
            foreach(var type in _requiredTypes)
            {
                _componentArrays[i++] = MultiComponentArray.CreateFrom(manager, type);
            }
        }

        public IComponentArray<T> GetComponentArray<T>() where T : struct, IComponent =>
            (IComponentArray<T>)_componentArrays[Array.IndexOf(_requiredTypes, typeof(T))];

        public bool DoesBlueprintMatch(Blueprint blueprint) => _requiredTypes.All(blueprint.Types.Contains);

        public void AddBlueprintContainer(BlueprintContainer container)
        {
            for(var i = 0; i < _requiredTypes.Length; ++i)
            {
                _componentArrays[i].Add(container.GetComponentArray(_requiredTypes[i]));
            }
        }
    }
}