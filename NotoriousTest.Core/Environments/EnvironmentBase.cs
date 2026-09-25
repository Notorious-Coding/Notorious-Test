using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Exceptions;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Infrastructures.Dependencies;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;

namespace NotoriousTest.Core.Environments;

/// <summary>
///     An environment is responsible for managing infrastructure lifetime, configuration, etc...
/// </summary>
public abstract class EnvironmentBase
{
    /// <summary>
    ///     List of all infrastructures within the environment.
    /// </summary>
    private readonly List<Infrastructure> _infrastructures = [];

    /// <summary>
    ///     List of all dependencies installed on the current environment.
    /// </summary>
    private List<IInfrastructureDependency> _installedDependencies = [];

    /// <summary>
    ///     Creates a new instance of the <see cref="EnvironmentBase" /> class.
    /// </summary>
    /// <param name="settings">Environment settings provider.</param>
    /// <param name="watchDog">Side recovery process interaction provider.</param>
    /// <param name="registry">Registry used to communicate with the watchdog provider.</param>
    /// <param name="runtime">Currently installed runtime provider.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="serviceProvider">DI Container.</param>
    public EnvironmentBase(EnvironmentSettings settings, IWatchDog watchDog, IRegistry registry, IRuntime runtime,
        ITestLogger logger, IServiceProvider serviceProvider)
    {
        Settings = settings;
        WatchDog = watchDog;
        Registry = registry;
        Runtime = runtime;
        Logger = logger;
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    ///     Gets the unique identifier for the environment instance.
    /// </summary>
    public EnvironmentId EnvironmentId { get; } = EnvironmentId.Create();

    /// <summary>
    ///     Current assembly running the environment.
    ///     In general, the only necessary implementation is Assembly.GetExecutingAssembly().
    /// </summary>
    protected abstract Assembly CurrentAssembly { get; }

    /// <summary>
    ///     Watchdog used to track infrastructure in case of crash.
    /// </summary>
    protected IWatchDog WatchDog { get; }

    /// <summary>
    ///     Registry shared between the Watchdog and the environment to communicate infrastructure information.
    /// </summary>
    protected IRegistry Registry { get; }

    /// <summary>
    ///     Runtime used to detect the current runtime of the test process.
    /// </summary>
    protected IRuntime Runtime { get; }

    /// <summary>
    ///     Test logger provider.
    /// </summary>
    protected ITestLogger Logger { get; }

    /// <summary>
    ///     Service provider used to resolve dependencies.
    /// </summary>
    protected IServiceProvider ServiceProvider { get; }

    /// <summary>
    ///     Settings for the running environment.
    /// </summary>
    protected EnvironmentSettings Settings { get; }


    /// <summary>
    ///     Configure this environment with infrastructures. Called before environment initialization.
    /// </summary>
    public abstract Task ConfigureEnvironment();


    /// <summary>
    ///     Get an infrastructure within the environment.
    /// </summary>
    /// <typeparam name="T">Infrastructure type</typeparam>
    /// <returns>Infrastructure of type <typeparamref name="T" /></returns>
    /// <exception cref="InfrastructureNotFoundException">Infrastructure has not beed found within environment.</exception>
    public T GetInfrastructure<T>() where T : Infrastructure
    {
        T? infrastructure = _infrastructures.OfType<T>().FirstOrDefault();

        if (infrastructure == null)
            throw new InfrastructureNotFoundException(
                $"L'infrastructure persistante de type {typeof(T)} n'éxiste pas, veuillez vérififer la méthode ${nameof(ConfigureEnvironment)}");

        return infrastructure;
    }

    /// <summary>
    ///     Add an infrastructure within the environment, using dependency injection.
    /// </summary>
    public EnvironmentBase AddInfrastructure<T>() where T : Infrastructure
        => AddInfrastructure(ActivatorUtilities.CreateInstance<T>(ServiceProvider, EnvironmentId));

    /// <summary>
    ///     Add an infrastructure within the environment.
    /// </summary>
    public EnvironmentBase AddInfrastructure(Infrastructure infrastructure)
    {
        infrastructure.EnvironmentId = EnvironmentId;
        infrastructure.WatchdogDisabled = Settings?.DisableWatchdog ?? false;
        _infrastructures.Add(infrastructure);
        return this;
    }

    /// <summary>
    ///     Initialize the environment and all infrastructures configured within it.
    /// </summary>
    public virtual async Task Initialize()
    {
        if (!Settings.DisableWatchdog)
        {
            await SetupRegistry();
            await StartDoggyDog();
        }

        await ConfigureEnvironment();

        await InstallAllMissingInfrastructureDependencies();
        await InitializeInfrastructureInParralelAndInOrder();
    }

    /// <summary>
    ///     Reset all infrastructures in the environment to a clean state.
    /// </summary>
    public virtual async Task Reset() =>
        await ExecuteActionOnInfrastructureInParralelAndInOrder(i => i.AutoReset ? i.ResetAsync() : Task.CompletedTask);

    /// <summary>
    ///     Destroy all infrastructures in the environment.
    /// </summary>
    public virtual async Task Destroy()
    {
        await ExecuteActionOnInfrastructureInParralelAndInOrder(i => i.DestroyAsync());
        await UninstallAllPreviouslyInstalledDependencies();

        if (!Settings.DisableWatchdog)
            WatchDog.SendSuccessSignal(EnvironmentId);
    }

    private async Task InstallAllMissingInfrastructureDependencies()
    {
        IEnumerable<IInfrastructureDependency> dependencies =
            _infrastructures.SelectMany(i => i.Dependencies).Distinct().ToList();

        bool[] exists = await Task.WhenAll(dependencies.Select(x => x.Exist()));

        var nonInstalledDependency = dependencies.Zip(exists, (d, e) => (Dependency: d, Exists: e))
            .Where(x => !x.Exists).Select(x => x.Dependency).ToList();
        await Task.WhenAll(nonInstalledDependency.Select(x => x.Install()));
    }

    private async Task UninstallAllPreviouslyInstalledDependencies()
    {
        IEnumerable<IInfrastructureDependency> dependencies =
            _infrastructures.SelectMany(i => i.Dependencies).Distinct().ToList();

        bool[] exists = await Task.WhenAll(dependencies.Select(x => x.Exist()));

        var nonInstalledDependency = dependencies.Zip(exists, (d, e) => (Dependency: d, Exists: e))
            .Where(x => !x.Exists).Select(x => x.Dependency).ToList();
        await Task.WhenAll(nonInstalledDependency.Select(x => x.Install()));
    }

    private async Task InitializeInfrastructureInParralelAndInOrder()
    {
        IEnumerable<IGrouping<int?, Infrastructure>> infrastructureGroupedByOrder =
            _infrastructures.OrderBy(i => i.Order).GroupBy(i => i.Order);

        var action = new Func<Infrastructure, Task>(async i =>
        {
            if (i is IConfigurationConsumer consumer)
                consumer.ConsumedConfiguration = AggregateInfrastructureConfiguration();
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
            Logger.Log(
                $"Runtime for {CurrentAssembly.FullName} cannot be found. Cleaner resolution may not work properly.",
                EnvironmentId);

        IEnumerable<string>? runtimesPath = runtimeConfiguration?.SupportedFrameworks?.Select(sf => sf.FilePath);
        WatchDog.Start(CurrentAssembly, Process.GetCurrentProcess().Id, EnvironmentId, runtimesPath);
    }


    private async Task ExecuteActionOnInfrastructureInParralelAndInOrder(Func<Infrastructure, Task> action)
    {
        IEnumerable<IGrouping<int?, Infrastructure>> infrastructureGroupedByOrder =
            _infrastructures.OrderBy(i => i.Order).GroupBy(i => i.Order);
        foreach (IGrouping<int?, Infrastructure> infrastructure in infrastructureGroupedByOrder)
            await Task.WhenAll(infrastructure.Select(action));
    }

    private List<ConfigurationEntry<object>> AggregateInfrastructureConfiguration() =>
        _infrastructures
            .Where(i => i is IConfigurationProducer)
            .SelectMany(i => (i as IConfigurationProducer)!.OutputConfiguration)
            .ToList();

    private Task SetupRegistry() => Registry.Ensure();
}
