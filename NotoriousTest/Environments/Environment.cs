using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Configuration;
using NotoriousTest.Exceptions;
using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;
using NotoriousTest.Settings;

using Xunit;
using Xunit.Sdk;

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
        public ContextId EnvironmentId { get; private set; } = Guid.NewGuid();

        /// <summary>Gets the service provider built from the configured infrastructure services.</summary>
        protected IServiceProvider? ServiceProvider { get; private set; }
        private IServiceCollection? _serviceCollection;

        /// <summary>
        /// Gets the collection of infrastructure components associated with this instance.
        /// </summary>
        private List<Infrastructure> _infrastructures = [];
        private readonly IMessageSink _sink;

        /// <summary>Initializes a new instance of <see cref="Environment"/> with the specified message sink.</summary>
        /// <param name="sink">The xUnit message sink used for diagnostic output.</param>
        protected Environment(IMessageSink sink)
        {
            _sink = sink;
        }
        #region IAsyncLifetime Implementation

        /// <summary>
        /// Initialize environment. THIS METHOD IS CALLED BY XUNIT, DO NOT USE IT.
        /// </summary>
        public async ValueTask InitializeAsync()
        {
            _serviceCollection = new ServiceCollection();
            await ConfigureInfrastructureServices(_serviceCollection);
            ServiceProvider = _serviceCollection.BuildServiceProvider();
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
        /// Configuration infrastructure dependency injection.
        /// </summary>
        /// <returns></returns>
        public virtual async Task ConfigureInfrastructureServices(IServiceCollection collection)
        {
            collection.AddSingleton(EnvironmentId);
            collection.AddSingleton<ITestSettingsProvider, TestSettingsProvider>();
            collection.AddSingleton(_sink);
            collection.AddSingleton<ITestLogger, TestLogger>();
        }

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
        public Environment AddInfrastructure<T>() where T : Infrastructure
            => AddInfrastructure(ActivatorUtilities.CreateInstance<T>(ServiceProvider!));

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        public Environment AddInfrastructure(Infrastructure infrastructure)
        {
            infrastructure.ContextId = EnvironmentId;
            _infrastructures.Add(infrastructure);
            return this;
        }

        /// <summary>Initializes all registered infrastructures in order, injecting consumed configuration where needed.</summary>
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

        /// <summary>Resets all infrastructures that have <see cref="IInfrastructure.AutoReset"/> enabled, in order.</summary>
        public virtual async Task Reset()
        {
            foreach (Infrastructure infrastructure in _infrastructures.OrderBy(pi => pi.Order))
            {
                if (infrastructure.AutoReset) await infrastructure.ResetAsync();
            }
        }

        /// <summary>Destroys all registered infrastructures in order.</summary>
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
                    .SelectMany(i => ((IConfigurationProducer)i).OutputConfiguration)
                    .ToList();
        }
    }
}
