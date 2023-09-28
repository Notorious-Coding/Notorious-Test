using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;
using Xunit;

namespace NotoriousTest.Environments
{
    public abstract class AsyncEnvironment : IAsyncLifetime
    {
        public Guid EnvironmentId => Guid.NewGuid();

        private List<AsyncInfrastructure> _infrastructures = new List<AsyncInfrastructure>();

        public abstract Task ConfigureEnvironmentAsync(AsyncEnvironmentConfig config);

        public async Task DisposeAsync()
        {
            foreach (AsyncInfrastructure infra in _infrastructures)
            {
                await infra.Destroy();
            }
        }

        public async Task<T> GetInfrastructureAsync<T>() where T : AsyncInfrastructure
        {
            T? infrastructure = _infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {nameof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironmentAsync)}");

            return infrastructure;
        }

        public async Task InitializeAsync()
        {
            // Called before test campaign
            var config = new AsyncEnvironmentConfig();
            await ConfigureEnvironmentAsync(config);

            _infrastructures = config.Infrastructures;

            foreach (AsyncInfrastructure infra in _infrastructures)
            {
                await infra.Initialize();
            }
        }

        public async Task ResetAsync()
        {
            foreach (AsyncInfrastructure infrastructure in _infrastructures.OrderBy(pi => pi.Order))
            {
                await infrastructure.Reset();
            }
        }
    }
}
