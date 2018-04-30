using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Mugen.Experimental.Test")]

namespace Mugen.Experimental
{
    using System;
    using System.Threading.Tasks;
    using Abstraction;
    using Abstraction.Systems;

    public class World : IDisposable
    {
        public IEntityManager EntityManager => _entityManager;
        
        private readonly  EntityManager _entityManager;
        private IUpdateSystemBase[] _updateSystems;
        private int _countSystems;

        public World()
        {
            _updateSystems = new IUpdateSystemBase[4];
            _entityManager = new EntityManager();
        }

        public async Task Update(float deltaTime)
        {
            for (var i = 0; i < _countSystems; ++i)
            {
                var system = _updateSystems[i];
                if (system.IsAsync) await ((AUpdateSystemAsync) system).Update(deltaTime);
                else ((AUpdateSystem) system).Update(deltaTime);
            }
        }

        public void AddSystem(IUpdateSystemBase system)
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
            var newArray = new IUpdateSystemBase[_countSystems * 2];
            Array.Copy(_updateSystems, newArray, _countSystems);
            _updateSystems = newArray;
        }

        public void Dispose()
        {
            _entityManager.Dispose();
        }
    }


}
