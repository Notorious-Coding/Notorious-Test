using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;

namespace NotoriousTest
{
    public abstract class Environment : IDisposable
    {
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

        private List<Infrastructure> _persistantInfrastructures = new List<Infrastructure>();

        public Environment()
        {
            // Called before test campaign
            var config = new EnvironmentConfig();
            ConfigureEnvironment(config);

            _persistantInfrastructures = config.PersistantInfrastructures;

            Initialize();
        }

        public abstract void ConfigureEnvironment(EnvironmentConfig config);
       
        public T GetPersistantInfrastructure<T>()
        {
            T? infrastructure = _persistantInfrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {nameof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

            return infrastructure;
        }

        public void Initialize()
        {
            foreach (Infrastructure infra in _persistantInfrastructures)
            {
                infra.Initialize();
            }
        }
        public void Reset()
        {
            foreach(Infrastructure infrastructure in _persistantInfrastructures.OrderBy(pi => pi.Order))
            {
                infrastructure.Reset();
            }
        }
        public void Dispose()
        {
            // Called after test campaign

            foreach (Infrastructure infra in _persistantInfrastructures)
            {
                infra.Destroy();
            }
        }
    }

    public class EnvironmentConfig
    {
        public List<Infrastructure> PersistantInfrastructures { get; private set; } = new List<Infrastructure>();

        public EnvironmentConfig AddPersistantInfrastructures(Infrastructure infrastructure)
        {
            PersistantInfrastructures.Add(infrastructure);
            return this;
        }
    }
}
