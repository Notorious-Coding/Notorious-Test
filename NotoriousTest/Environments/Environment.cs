using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;

namespace NotoriousTest.Environments
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

            _infrastructures = config.Infrastructures;

            Initialize();
        }

        public abstract void ConfigureEnvironment(EnvironmentConfig config);

        public T GetInfrastructure<T>() where T : Infrastructure
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
            foreach (Infrastructure infrastructure in _infrastructures.OrderBy(pi => pi.Order))
            {
                infrastructure.Reset();
            }
        }

        public void Dispose()
        {
            foreach (Infrastructure infra in _infrastructures)
            {
                infra.Destroy();
            }
        }
    }
}
