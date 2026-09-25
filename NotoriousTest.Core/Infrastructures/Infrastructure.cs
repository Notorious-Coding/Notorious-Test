using System.Diagnostics;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Infrastructures.Dependencies;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Core.Infrastructures;

/// <inheritdoc cref="Infrastructure{TMetadata}" />
/// <typeparam name="TOutputConfiguration">Configuration type produced by the infrastructure.</typeparam>
public abstract class Infrastructure<TOutputConfiguration, TMetadata> : Infrastructure<TMetadata>,
    IConfigurationProducer<TOutputConfiguration>
    where TMetadata : class
{
    protected Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger,
        provider)
    {
    }

    public List<IInfrastructureDependency> Dependencies { get; }
    public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration { get; } = new();

    public void AddEntry(string key, TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, key));

    public void AddEntry(TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, value!.GetType().Name));
}

/// <inheritdoc />
/// <typeparam name="TMetadata">
///     Metadata containing relevant access configuration.
///     Used by the watchdog to recover this infrastructure in case of crash.
/// </typeparam>
public abstract class Infrastructure<TMetadata> : Infrastructure where TMetadata : class
{
    /// <inheritdoc />
    protected Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger,
        provider)
    {
    }

    /// <summary>
    ///     Metadata containing relevant access configuration.
    /// </summary>
    public new TMetadata? Metadata
    {
        get => (TMetadata?)base.Metadata;
        protected set => base.Metadata = value;
    }
}

/// <summary>
///     Infrastructure is a base class to define a test infrastructure.
/// </summary>
public abstract class Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider)
    : IAsyncDisposable, IInfrastructure
{
    public Guid Id = Guid.NewGuid();

    /// <summary>
    ///     Watchdog will not keep track of this infrastructure if set to true.
    /// </summary>
    public virtual bool DisableRegistry => false;

    /// <summary>
    ///     Watchdog will not keep track of this infrastructure if set to true.
    /// </summary>
    internal bool WatchdogDisabled { get; set; }

    /// <summary>
    ///     Gets or sets the metadata associated with the current object.
    /// </summary>
    public object? Metadata { get; protected set; }

    /// <summary>
    ///     Gets the logger instance used to record test execution details and diagnostic information.
    /// </summary>
    protected ITestLogger Logger { get; } = logger;

    /// <summary>
    ///     Gets the registry provider used to track infrastructure and clean them after test crash.
    /// </summary>
    protected IRegistry Registry { get; } = provider;

    /// <summary>
    ///     This infrastructure has been registered in the registry if set to true.
    /// </summary>
    protected bool Registered { get; private set; }

    public async ValueTask DisposeAsync() => await DestroyAsync();

    /// <inheritdoc />
    public List<IInfrastructureDependency> Dependencies { get; set; } = new();

    /// <inheritdoc />
    public virtual int? Order { get; }

    /// <inheritdoc />
    public bool AutoReset { get; set; } = true;

    /// <inheritdoc />
    public EnvironmentId EnvironmentId { get; set; } = contextId;

    /// <inheritdoc />
    public abstract Task Initialize();

    /// <inheritdoc />
    public virtual Task Reset() => Task.CompletedTask;

    /// <inheritdoc />
    public abstract Task Destroy();

    /// <summary>
    ///     Register the infrastructure in the watchdog registry.
    /// </summary>
    protected async Task Register()
    {
        await Registry.Register(new InfrastuctureRegistryEntry
        {
            InfrastructureId = Id,
            InfrastructureType = GetType(),
            Metadata = Metadata,
            EnvironmentId = EnvironmentId,
            ProcessID = Process.GetCurrentProcess().Id
        });

        Registered = true;
    }

    internal async Task InitializeAsync()
    {
        try
        {
            Logger.Log($"[{GetType().Name}] Initialization ...", EnvironmentId);
            var sw = Stopwatch.StartNew();

            await Initialize();
            if (!WatchdogDisabled && !DisableRegistry && !Registered) await Register();

            Logger.Log($"[{GetType().Name}] Initialization completed in {sw.ElapsedMilliseconds} ms", EnvironmentId);
        }
        catch (Exception ec)
        {
            Logger.Log("Initialization failed with exception: " + ec, EnvironmentId);
            await Destroy();
            throw;
        }
    }

    internal async Task ResetAsync()
    {
        Logger.Log($"[{GetType().Name}] Reset ...", EnvironmentId);
        var sw = Stopwatch.StartNew();

        await Reset();
        if (!WatchdogDisabled && !DisableRegistry) await Registry.NotifyReset(Id);
        Logger.Log($"[{GetType().Name} ] Reset completed in {sw.ElapsedMilliseconds} ms", EnvironmentId);
    }

    internal async Task DestroyAsync()
    {
        Logger.Log($"[{GetType().Name}] Destroy ...", EnvironmentId);
        var sw = Stopwatch.StartNew();

        await Destroy();
        if (!WatchdogDisabled && !DisableRegistry) await Registry.Remove(Id);

        Logger.Log($"[{GetType().Name} ] Destroy completed in {sw.ElapsedMilliseconds} ms", EnvironmentId);
    }
}
