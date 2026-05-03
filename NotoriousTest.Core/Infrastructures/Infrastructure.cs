using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;

using System.Diagnostics;

namespace NotoriousTest.Core.Infrastructures;


public abstract class Infrastructure<TOutputConfiguration, TMetadata> : Infrastructure<TMetadata>, IConfigurationProducer<TOutputConfiguration>
    where TMetadata : class
{
    public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration { get; } = new();

    protected Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
    {
    }

    public void AddEntry(string key, TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, key));

    public void AddEntry(TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, value!.GetType().Name));
}

public abstract class Infrastructure<TMetadata> : Infrastructure where TMetadata : class
{
    /// <summary>
    /// Gets or sets the metadata associated with the current object.
    /// </summary>
    public new TMetadata? Metadata { get => (TMetadata?)base.Metadata; protected set => base.Metadata = value; }

    protected Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
    {
    }
}

/// <summary>
/// Infrastructure is a base class to define a test infrastructure.
/// </summary>
public abstract class Infrastructure : IAsyncDisposable, IInfrastructure
{
    ///<inheritdoc/>
    public virtual int? Order { get; }

    ///<inheritdoc/>
    public bool AutoReset { get; set; } = true;

    public virtual bool DisableRegistry => false;
    internal bool WatchdogDisabled { get; set; }
    ///<inheritdoc/>
    public EnvironmentId EnvironmentId { get; set; }
    public Guid Id = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the metadata associated with the current object.
    /// </summary>
    public object? Metadata { get; protected set; }
    /// <summary>
    /// Gets the logger instance used to record test execution details and diagnostic information.
    /// </summary>
    protected ITestLogger Logger { get; }

    /// <summary>
    /// Gets the registry provider used to track infrastructure and clean them after test crash.
    /// </summary>
    protected IRegistry Registry { get; }
    protected bool Registered { get; private set; }

    public Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider)
    {
        EnvironmentId = contextId;
        Logger = logger;
        Registry = provider;
    }

    public abstract Task Initialize();
    public virtual Task Reset() => Task.CompletedTask;
    public abstract Task Destroy();

    public async ValueTask DisposeAsync()
    {
        await DestroyAsync();
    }

    internal async Task InitializeAsync()
    {
        try
        {
            Logger.Log($"[{GetType().Name}] Initialization ...");
            var sw = Stopwatch.StartNew();

            await Initialize();
            if (!WatchdogDisabled && !DisableRegistry && !Registered) await Register();

            Logger.Log($"[{GetType().Name}] Initialization completed in {sw.ElapsedMilliseconds} ms");

        }
        catch (Exception ec)
        {
            Logger.Log("Initialization failed with exception: " + ec.ToString());
            await Destroy();
            throw;
        }
    }

    protected async Task Register()
    {
        await Registry.Register(new InfrastuctureRegistryEntry()
        {
            InfrastructureId = Id,
            InfrastructureType = GetType(),
            Metadata = Metadata,
            EnvironmentId = EnvironmentId,
            ProcessID = Process.GetCurrentProcess().Id,
        });

        Registered = true;
    }

    internal async Task ResetAsync()
    {
        Logger.Log($"[{GetType().Name}] Reset ...");
        var sw = Stopwatch.StartNew();

        await Reset();
        if (!WatchdogDisabled && !DisableRegistry) await Registry.NotifyReset(Id);
        Logger.Log($"[{GetType().Name} ] Reset completed in {sw.ElapsedMilliseconds} ms");

    }

    internal async Task DestroyAsync()
    {
        Logger.Log($"[{GetType().Name}] Destroy ...");
        var sw = Stopwatch.StartNew();

        await Destroy();
        if (!WatchdogDisabled && !DisableRegistry) await Registry.Remove(Id);

        Logger.Log($"[{GetType().Name} ] Destroy completed in {sw.ElapsedMilliseconds} ms");
    }
}
