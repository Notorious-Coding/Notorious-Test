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
        public EnvironmentId EnvironmentId { get;  } = EnvironmentId.Create();
        public abstract Assembly CurrentAssembly { get; }
        protected IWatchDog WatchDog { get; }
        protected IRegistry Registry { get; }
        protected IRuntime Runtime { get;  }
        protected ITestLogger Logger { get;  }
        protected IServiceProvider ServiceProvider { get; }

        protected EnvironmentSettings Settings { get;  }

        public EnvironmentBase(EnvironmentSettings settings, IWatchDog watchDog, IRegistry registry, IRuntime runtime, ITestLogger logger, IServiceProvider serviceProvider)
        {
            Settings = settings;
            WatchDog = watchDog;
            Registry = registry;
            Runtime = runtime;
            Logger = logger;
            ServiceProvider = serviceProvider;
        }
        /// <summary>
        /// Gets the collection of infrastructure components associated with this instance.
        /// </summary>
        private List<Infrastructure> _infrastructures = [];


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
            => AddInfrastructure(ActivatorUtilities.CreateInstance<T>(ServiceProvider, EnvironmentId));

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
            // Setup registry
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
            IEnumerable<IGrouping<int?, Infrastructure>> infrastructureGroupedByOrder = _infrastructures.OrderBy(i => i.Order).GroupBy(i => i.Order);

            var action = new Func<Infrastructure, Task>(async (i) =>
            {
                if (i is IConfigurationConsumer consumer)
                {
                    consumer.ConsumedConfiguration = AggregateInfrastructureConfiguration();
                }
                await i.InitializeAsync();
            });

            foreach (IGrouping<int?, Infrastructure> infrastructureGroup in infrastructureGroupedByOrder)
            {
                IEnumerable<Infrastructure> nonConsumers = infrastructureGroup.Where(i => i is not IConfigurationConsumer);
                IEnumerable<Infrastructure> consumers = infrastructureGroup.Where(i => i is IConfigurationConsumer);
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

        public virtual async Task Reset() => await ExecuteActionOnInfrastructureInParralelAndInOrder((i) => i.AutoReset ? i.ResetAsync() : Task.CompletedTask);

        public virtual async Task Destroy()
        {

            await ExecuteActionOnInfrastructureInParralelAndInOrder((i) => i.DestroyAsync());

            if (!Settings.DisableWatchdog)
                WatchDog.SendSuccessSignal(EnvironmentId);
        }

        private async Task ExecuteActionOnInfrastructureInParralelAndInOrder(Func<Infrastructure, Task> action)
        {
            IEnumerable<IGrouping<int?, Infrastructure>> infrastructureGroupedByOrder = _infrastructures.OrderBy(i => i.Order).GroupBy(i => i.Order);
            foreach (IGrouping<int?, Infrastructure> infrastructure in infrastructureGroupedByOrder)
            {
                await Task.WhenAll(infrastructure.Select(action));
            }
        }

        private List<ConfigurationEntry<object>> AggregateInfrastructureConfiguration() =>
            _infrastructures
                .Where(i => i is IConfigurationProducer)
                .SelectMany(i => (i as IConfigurationProducer)!.OutputConfiguration)
                .ToList();

        private Task SetupRegistry() => Registry.Ensure();
    }
}
