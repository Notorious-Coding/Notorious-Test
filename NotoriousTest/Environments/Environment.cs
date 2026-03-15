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
        public Guid EnvironmentId { get; private set; } = Guid.NewGuid();
        public List<ConfigurationEntry<object>> OutputConfiguration { get; set; } = new();

        protected List<IInfrastructure> Infrastructures { get; private set; } = new List<IInfrastructure>();
        #region IAsyncLifetime Implementation

        /// <summary>
        /// Initialize environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async Task InitializeAsync()
        {
            await ConfigureEnvironment();
            await Initialize();
        }

        /// <summary>
        /// Destroy environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async Task DisposeAsync()
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
        public T GetInfrastructure<T>() where T : IInfrastructure
        {
            T? infrastructure = this.Infrastructures.OfType<T>().FirstOrDefault();

            if (infrastructure == null) throw new InfrastructureNotFoundException($"L'infrastructure persistante de type {typeof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

            return infrastructure;
        }

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        /// <param name="infrastructure"></param>
        public Environment AddInfrastructure(IInfrastructure infrastructure)
        {
            infrastructure.ContextId = EnvironmentId;
            Infrastructures.Add(infrastructure);
            return this;
        }

        public virtual async Task Initialize()
        {
            foreach (IInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurationConsumer consumer)
                {
                    consumer.ConsumedConfiguration = AggregateInfrastructureConfiguration();
                }

                await infra.Initialize();

                if (infra is IConfigurationProducer producer)
                {
                    if (producer.OutputConfiguration is not null)
                    {
                        OutputConfiguration.AddRange(producer.OutputConfiguration);
                    }
                }
            }
        }

        public virtual async Task Reset()
        {
            foreach (IInfrastructure infrastructure in Infrastructures.OrderBy(pi => pi.Order))
            {
                if (infrastructure.AutoReset) await infrastructure.Reset();
            }
        }

        public virtual async Task Destroy()
        {
            foreach (IInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                await infra.Destroy();
            }
        }

        private List<ConfigurationEntry<object>> AggregateInfrastructureConfiguration()
        {
            return Infrastructures
                    .Where(i => i is IConfigurationProducer)
                    .SelectMany(i => (i as IConfigurationProducer).OutputConfiguration)
                    .ToList();
        }
    }
}
