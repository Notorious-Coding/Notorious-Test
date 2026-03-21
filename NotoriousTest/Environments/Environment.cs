using NotoriousTest.Configuration;
using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;

using Xunit;

namespace NotoriousTest.Environments
{

    /// <summary>
    /// An environment is  responsible of managing infrastructure lifetime, configuration, etc...
    /// </summary>
    public abstract class Environment : IAsyncLifetime
    {
        /// <summary>
        /// Gets the unique identifier for the environment instance.
        /// </summary>
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// Gets the collection of infrastructure components associated with this instance.
        /// </summary>
        private List<Infrastructure> _infrastructures = [];
        #region IAsyncLifetime Implementation

        /// <summary>
        /// Initialize environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async ValueTask InitializeAsync()
        {
            await ConfigureEnvironment();
            await Initialize();
        }

        /// <summary>
        /// Destroy environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await Destroy();
        }
        #endregion

        /// <summary>
        /// Configure environment with infrastructures. Called before environment initialization.
        /// </summary>
        public abstract Task ConfigureEnvironment();


        /// <summary>
        /// Get an infrastructure within environment.
        /// </summary>
        /// <typeparam name="T">Infrastructure type</typeparam>
        /// <returns>Infrastructure of type <typeparamref name="T"/></returns>
        /// <exception cref="InfrastructureNotFoundException">Infrastructure has not beed found within environment.</exception>
        public T GetInfrastructure<T>() where T : Infrastructure
        {
            T? infrastructure = _infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {typeof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

            return infrastructure;
        }

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        public Environment AddInfrastructure<T>() where T : Infrastructure, new()
            => AddInfrastructure(Activator.CreateInstance<T>());

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        public Environment AddInfrastructure(Infrastructure infrastructure)
        {
            infrastructure.ContextId = EnvironmentId;
            _infrastructures.Add(infrastructure);
            return this;
        }

        public virtual async Task Initialize()
        {
            foreach (Infrastructure infra in _infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurationConsumer consumer)
                {
                    consumer.ConsumedConfiguration = AggregateInfrastructureConfiguration();
                }

                await infra.InitializeAsync();
            }
        }

        public virtual async Task Reset()
        {
            foreach (Infrastructure infrastructure in _infrastructures.OrderBy(pi => pi.Order))
            {
                if (infrastructure.AutoReset) await infrastructure.ResetAsync();
            }
        }

        public virtual async Task Destroy()
        {
            foreach (Infrastructure infra in _infrastructures.OrderBy(i => i.Order))
            {
                await infra.Destroy();
            }
        }

        private List<ConfigurationEntry<object>> AggregateInfrastructureConfiguration()
        {
            return _infrastructures
                    .Where(i => i is IConfigurationProducer)
                    .SelectMany(i => (i as IConfigurationProducer).OutputConfiguration)
                    .ToList();
        }
    }
}
