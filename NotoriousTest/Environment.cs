using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;

namespace NotoriousTest
{
    public abstract class Environment : IDisposable
    {
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

        private List<Infrastructure> _infrastructures = new List<Infrastructure>();

        public Environment()
        {
            // Called before test campaign
            var config = new EnvironmentConfig();
            ConfigureEnvironment(config);

            _infrastructures = config.PersistantInfrastructures;

            Initialize();
        }

        public abstract void ConfigureEnvironment(EnvironmentConfig config);
       
        public T GetInfrastructure<T>()
        {
            T? infrastructure = _infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {nameof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

            return infrastructure;
        }

        public void Initialize()
        {
            foreach (Infrastructure infra in _infrastructures)
            {
                infra.Initialize();
            }
        }
        public void Reset()
        {
            foreach(Infrastructure infrastructure in _infrastructures.OrderBy(pi => pi.Order))
            {
                infrastructure.Reset();
            }
        }
        public void Dispose()
        {
            // Called after test campaign

            foreach (Infrastructure infra in _infrastructures)
            {
                infra.Destroy();
            }
        }
    }

    public class EnvironmentConfig
    {
        public List<Infrastructure> PersistantInfrastructures { get; private set; } = new List<Infrastructure>();

        public EnvironmentConfig AddInfrastructures(Infrastructure infrastructure)
        {
            PersistantInfrastructures.Add(infrastructure);
            return this;
        }
    }
}
