namespace Mugen
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class World
    {
        private readonly ISystemFactory _systemFactory;
        private readonly List<IUpdateSystemBase> _updateSystems;

        public IEntityManager EntityManager { get; }

        public World(ISystemFactory systemFactory)
        {
            _systemFactory = systemFactory;
            _updateSystems = new List<IUpdateSystemBase>();
            EntityManager = new EntityManager();
        }

        public World AddSystem<T>()
        {
            return AddSystem(_systemFactory.Create<T>(this));
        }

        public World AddSystem(ISystem system)
        {
            if(system.HasUpdateMethod)
            {
                _updateSystems.Add((IUpdateSystemBase)system);
            }

            return this;
        }

        public async Task Update(float deltaTime)
        {
            foreach(var system in _updateSystems)
            {
                if(system.IsAsync)
                {
                    await ((IUpdateSystemAsync)system).Update(deltaTime);
                }
                else
                {
                    ((IUpdateSystem)system).Update(deltaTime);
                }
            }
        }
    }
}