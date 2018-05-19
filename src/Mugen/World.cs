using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Mugen.Test")]

namespace Mugen
{
    using System;
    using Abstraction;
    using Abstraction.Systems;

    public class World : IDisposable
    {
        private readonly EntityManager _entityManager;
        private int _countSystems;
        private AUpdateSystem[] _updateSystems;

        public World()
        {
            _updateSystems = new AUpdateSystem[4];
            _entityManager = new EntityManager();
        }

        public IEntityManager EntityManager => _entityManager;

        public void Dispose()
        {
            _entityManager.Dispose();
        }

        public void Update(float deltaTime)
        {
            for (var i = 0; i < _countSystems; ++i)
            {
                _updateSystems[i].Update(deltaTime);
            }
        }

        public void AddSystem(AUpdateSystem system)
        {
            if (_updateSystems.Length <= _countSystems)
            {
                ResizeArray();
            }

            _updateSystems[_countSystems] = system;
            ++_countSystems;
        }

        private void ResizeArray()
        {
            var newArray = new AUpdateSystem[_countSystems + 4];
            Array.Copy(_updateSystems, newArray, _countSystems);
            _updateSystems = newArray;
        }
    }
}