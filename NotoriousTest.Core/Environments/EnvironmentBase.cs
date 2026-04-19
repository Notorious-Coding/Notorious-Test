using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Exceptions;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Settings;
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
        public EnvironmentId EnvironmentId { get; private set; } = Guid.NewGuid();
        public abstract Assembly CurrentAssembly { get; }

        protected IServiceProvider ServiceProvider { get; private set; }
        private IServiceCollection _serviceCollection;
        protected IWatchDog WatchDog => _watchDog ??= ServiceProvider.GetRequiredService<IWatchDog>();
        private IWatchDog? _watchDog;
        protected IRegistry Registry => _registry ??= ServiceProvider.GetRequiredService<IRegistry>();
        private IRegistry? _registry;
        protected IRuntime Runtime => _runtime ??= ServiceProvider.GetRequiredService<IRuntime>();
        private IRuntime? _runtime;
        protected ITestLogger Logger => _logger ??= ServiceProvider.GetRequiredService<ITestLogger>();
        private ITestLogger _logger;

        protected EnvironmentConfiguration Settings => _settings ??= ServiceProvider.GetRequiredService<ITestSettingsProvider>()?.Get<EnvironmentConfiguration>(EnvironmentConfiguration.SECTION_NAME) ?? new EnvironmentConfiguration();
        private EnvironmentConfiguration _settings;


        /// <summary>
        /// Define current test assembly.
        /// </summary>

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
            infrastructure.EnvironmentId = EnvironmentId;
            infrastructure.WatchdogDisabled = Settings?.DisableWatchdog ?? false;
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
            if (!Settings.DisableWatchdog)
            {
                await SetupRegistry();
                await StartDoggyDog();
            }
            await ConfigureEnvironment();

            await InitializeInfrastructureInParralelAndInOrder();
        }

        private async Task InitializeInfrastructureInParralelAndInOrder()
        {
            var infrastructureGroupedByOrder = _infrastructures.OrderBy(i => i.Order).GroupBy(i => i.Order);

            var action = new Func<Infrastructure, Task>(async (i) =>
            {
                if (i is IConfigurationConsumer consumer)
                {
                    consumer.ConsumedConfiguration = AggregateInfrastructureConfiguration();
                }
                await i.InitializeAsync();
            });

            foreach (var infrastructureGroup in infrastructureGroupedByOrder)
            {
                var nonConsumers = infrastructureGroup.Where(i => i is not IConfigurationConsumer);
                var consumers = infrastructureGroup.Where(i => i is IConfigurationConsumer);
                await Task.WhenAll(nonConsumers.Select(i => action(i)));
                await Task.WhenAll(consumers.Select(i => action(i)));
            }
        }

        private async Task StartDoggyDog()
        {
            RuntimeConfiguration? runtimeConfiguration = Runtime.GetSupportedRuntimes(CurrentAssembly);
            if (runtimeConfiguration == null)
                Logger.Log($"Runtime for {CurrentAssembly.FullName} cannot be found. Cleaner resolution may not work properly.");

            IEnumerable<string>? runtimesPath = runtimeConfiguration?.SupportedFrameworks?.Select(sf => sf.FilePath);
            WatchDog.Start(CurrentAssembly, Process.GetCurrentProcess().Id, EnvironmentId, runtimesPath);
        }

        public virtual async Task Reset()
        {
            await ExecuteActionOnInfrastructureInParralelAndInOrder((i) => i.AutoReset ? i.ResetAsync() : Task.CompletedTask);

        }

        public virtual async Task Destroy()
        {
            await ExecuteActionOnInfrastructureInParralelAndInOrder((i) => i.DestroyAsync());

            if (!Settings.DisableWatchdog)
                WatchDog.SendSuccessSignal(EnvironmentId);
        }

        private async Task ExecuteActionOnInfrastructureInParralelAndInOrder(Func<Infrastructure, Task> action)
        {
            var infrastructureGroupedByOrder = _infrastructures.OrderBy(i => i.Order).GroupBy(i => i.Order);
            foreach (var infrastructure in infrastructureGroupedByOrder)
            {
                await Task.WhenAll(infrastructure.Select(action));
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
