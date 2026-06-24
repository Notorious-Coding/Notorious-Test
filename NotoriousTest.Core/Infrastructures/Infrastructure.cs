using System.Diagnostics;
using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

namespace NotoriousTest.Core.Infrastructures;

public abstract class Infrastructure<TOutputConfiguration, TMetadata> : Infrastructure<TMetadata>,
    IConfigurationProducer<TOutputConfiguration>
    where TMetadata : class
{
    protected Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider,
        EnvironmentSettings settings) : base(contextId, logger, provider, settings)
    {
    }

    public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration { get; } = new();

    public void AddEntry(string key, TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, key));

    public void AddEntry(TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, value!.GetType().Name));
}

public abstract class Infrastructure<TMetadata> : Infrastructure where TMetadata : class
{
    protected Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider,
        EnvironmentSettings settings) : base(contextId, logger, provider, settings)
    {
    }

    /// <summary>
    ///     Gets or sets the metadata associated with the current object.
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
public abstract class Infrastructure : IAsyncDisposable, IInfrastructure
{
    private readonly EnvironmentSettings _settings;

    public Guid Id = Guid.NewGuid();


    public Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider, EnvironmentSettings settings)
    {
        _settings = settings;
        EnvironmentId = contextId;
        Logger = logger;
        Registry = provider;
    }

    /// <summary>
    ///     Gets or sets the metadata associated with the current object.
    /// </summary>
    public object? Metadata { get; protected set; }

    /// <summary>
    ///     Gets the logger instance used to record test execution details and diagnostic information.
    /// </summary>
    protected ITestLogger Logger { get; }

    /// <summary>
    ///     Gets the registry provider used to track infrastructure and clean them after test crash.
    /// </summary>
    protected IRegistry Registry { get; }

    protected bool Registered { get; private set; }
    protected virtual bool DisableRegistry => false;

    public async ValueTask DisposeAsync() => await DestroyAsync();

    /// <inheritdoc />
    public virtual int? Order { get; }

    /// <inheritdoc />
    public bool AutoReset { get; set; } = true;

    /// <inheritdoc />
    public EnvironmentId EnvironmentId { get; set; }

    public abstract Task Initialize();
    public virtual Task Reset() => Task.CompletedTask;
    public abstract Task Destroy();

    internal async Task InitializeAsync()
    {
        try
        {
            Logger.Log($"[{GetType().Name}] Initialization ...", EnvironmentId);
            var sw = Stopwatch.StartNew();

            await Initialize();
            if (!_settings.DisableWatchdog && !DisableRegistry && !Registered) await Register();

            Logger.Log($"[{GetType().Name}] Initialization completed in {sw.ElapsedMilliseconds} ms", EnvironmentId);
        }
        catch (Exception ec)
        {
            Logger.Log("Initialization failed with exception: " + ec, EnvironmentId);
            await Destroy();
            throw;
        }
    }

    protected async Task Register()
    {
        await Registry.Register(new InfrastuctureRegistryEntry
        {
            InfrastructureId = Id,
            InfrastructureType = GetType(),
            Metadata = Metadata,
            EnvironmentId = EnvironmentId,
            ProcessID = Process.GetCurrentProcess().Id,
            ProcessName = IntegrationTestContext.Current.CurrentAssembly.GetName().Name
        });
        Registered = true;
    }

    internal async Task ResetAsync()
    {
        Logger.Log($"[{GetType().Name}] Reset ...", EnvironmentId);
        var sw = Stopwatch.StartNew();

        await Reset();
        if (!_settings.DisableWatchdog && !DisableRegistry) await Registry.NotifyReset(Id);
        Logger.Log($"[{GetType().Name} ] Reset completed in {sw.ElapsedMilliseconds} ms", EnvironmentId);
    }

    internal async Task DestroyAsync()
    {
        Logger.Log($"[{GetType().Name}] Destroy ...", EnvironmentId);
        var sw = Stopwatch.StartNew();

        await Destroy();
        if (!_settings.DisableWatchdog && !DisableRegistry) await Registry.Remove(Id);

        Logger.Log($"[{GetType().Name} ] Destroy completed in {sw.ElapsedMilliseconds} ms", EnvironmentId);
    }
}
