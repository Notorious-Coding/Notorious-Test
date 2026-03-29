using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Exceptions;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Watchdog;

using System.Diagnostics;
using System.Reflection;

namespace NotoriousTest.Core.Environments
{

    /// <summary>
    /// An environment is  responsible of managing infrastructure lifetime, configuration, etc...
    /// </summary>
    public abstract class EnvironmentBase
    {
        /// <summary>
        /// Gets the unique identifier for the environment instance.
        /// </summary>
        public ContextId EnvironmentId { get; private set; } = Guid.NewGuid();
        protected IServiceProvider ServiceProvider { get; private set; }
        private IServiceCollection _serviceCollection;

        /// <summary>
        /// Define current test assembly.
        /// </summary>
        public abstract Assembly CurrentAssembly { get; }

        /// <summary>
        /// Gets the collection of infrastructure components associated with this instance.
        /// </summary>
        private List<Infrastructure> _infrastructures = [];

        /// <summary>
        /// Configuration infrastructure dependency injection.
        /// </summary>
        /// <returns></returns>
        public virtual void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            _serviceCollection.AddSingleton(EnvironmentId);
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
        public EnvironmentBase AddInfrastructure<T>() where T : Infrastructure
            => AddInfrastructure(ActivatorUtilities.CreateInstance<T>(ServiceProvider));

        /// <summary>
        /// Add an infrastructure within environment.
        /// </summary>
        public EnvironmentBase AddInfrastructure(Infrastructure infrastructure)
        {
            infrastructure.ContextId = EnvironmentId;
            _infrastructures.Add(infrastructure);
            return this;
        }

        /// <summary>
        /// Initializes the infrastructure and configures all registered services asynchronously.
        /// </summary>
        /// <remarks>This method sets up the service collection, configures infrastructure services,
        /// builds the service provider, and initializes all registered infrastructure components in order. It should be
        /// called before using any dependent services or infrastructure components.</remarks>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        public virtual async Task Initialize()
        {
            _serviceCollection = new ServiceCollection();
            // Setup registry
            ConfigureInfrastructureServices(_serviceCollection);
            ServiceProvider = _serviceCollection.BuildServiceProvider();
            await SetupRegistry();
            await StartDoggyDog();
            await ConfigureEnvironment();

            foreach (Infrastructure infra in _infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurationConsumer consumer)
                {
                    consumer.ConsumedConfiguration = AggregateInfrastructureConfiguration();
                }

                await infra.InitializeAsync();
            }
        }

        private async Task StartDoggyDog()
        {
            var watchDog = ServiceProvider.GetRequiredService<IWatchDog>();
            watchDog.Start(CurrentAssembly, Process.GetCurrentProcess().Id);
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
                await infra.DestroyAsync();
            }
        }

        private List<ConfigurationEntry<object>> AggregateInfrastructureConfiguration()
        {
            return _infrastructures
                    .Where(i => i is IConfigurationProducer)
                    .SelectMany(i => (i as IConfigurationProducer).OutputConfiguration)
                    .ToList();
        }

        private async Task SetupRegistry()
        {
            var registry = ServiceProvider.GetRequiredService<IRegistry>();
            await registry.Ensure();
        }
    }
}
