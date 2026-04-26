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

    public virtual bool DisableRegistry { get; } = false;
    internal bool WatchdogDisabled { get; set; } = false;
    ///<inheritdoc/>
    public EnvironmentId EnvironmentId { get; set; }
    public Guid Id = Guid.NewGuid();
    private readonly List<IInfrastructureExtension> _extensions = new();

    /// <summary>
    /// Gets or sets the metadata associated with the current object.
    /// </summary>
    public object? Metadata { get; protected set; }
    /// <summary>
    /// Gets the logger instance used to record test execution details and diagnostic information.
    /// </summary>
    protected ITestLogger Logger { get; private set; }

    /// <summary>
    /// Gets the registry provider used to track infrastructure and clean them after test crash.
    /// </summary>
    protected IRegistry Registry { get; private set; }
    protected bool Registered { get; private set; } = false;

    public Infrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry provider)
    {
        EnvironmentId = contextId;
        Logger = logger;
        Registry = provider;
    }

    public abstract Task Initialize();
    public virtual Task Reset()
    {
        return Task.CompletedTask;
    }
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
            foreach (var extension in _extensions)
            {
                Logger.Log($"[{extension.GetType().Name}] OnBeforeInitialize");
                await extension.OnBeforeInitialize(this);
            }

            await Initialize();
            if (!WatchdogDisabled && !DisableRegistry && !Registered) await Register();


            foreach (var extension in _extensions)
            {
                Logger.Log($"[{extension.GetType().Name}] OnAfterInitialize");
                await extension.OnAfterInitialize(this);
            }
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
        foreach (var extension in _extensions)
        {
            Logger.Log($"[{extension.GetType().Name}] OnBeforeReset");
            await extension.OnBeforeReset(this);
        }

        await Reset();
        if (!WatchdogDisabled && !DisableRegistry) await Registry.NotifyReset(Id);
        foreach (var extension in _extensions)
        {
            Logger.Log($"[{extension.GetType().Name}] OnAfterReset");
            await extension.OnAfterReset(this);
        }
        Logger.Log($"[{GetType().Name} ] Reset completed in ");

    }

    internal async Task DestroyAsync()
    {
        Logger.Log($"[{GetType().Name}] Destroy ...");
        var sw = Stopwatch.StartNew();

        foreach (var extension in _extensions)
        {
            Logger.Log($"[{extension.GetType().Name}] OnBeforeDestroy");
            await extension.OnBeforeDestroy(this);
        }

        await Destroy();
        if (!WatchdogDisabled && !DisableRegistry) await Registry.Remove(Id);

        foreach (var extension in _extensions)
        {
            Logger.Log($"[{extension.GetType().Name}] OnAfterDestroy");
            await extension.OnAfterDestroy(this);
        }

        Logger.Log($"[{GetType().Name} ] Destroy completed in ");
    }

    /// <summary>
    /// Ensures that an extension of the specified type is present in the collection, returning the existing instance if
    /// found or adding and returning the provided instance if not.
    /// </summary>
    /// <typeparam name="T">The type of the infrastructure extension to ensure. Must implement IInfrastructureExtension.</typeparam>
    /// <param name="extension">The extension instance to add if an existing instance of type T is not already present. Cannot be null.</param>
    /// <returns>The existing extension of type T if present; otherwise, the provided extension instance.</returns>
    public T EnsureExtension<T>(T extension) where T : IInfrastructureExtension
    {
        var existing = _extensions.OfType<T>().FirstOrDefault();
        if (existing != null) return existing;

        _extensions.Add(extension);
        return extension;
    }

    /// <summary>
    /// Retrieves an existing extension of the specified type from the collection, or creates and adds a new instance if
    /// none exists.
    /// </summary>
    /// <typeparam name="T">The type of extension to retrieve or create. Must implement IInfrastructureExtension and have a parameterless
    /// constructor.</typeparam>
    /// <returns>An instance of the specified extension type. If an extension of this type already exists in the collection, it
    /// is returned; otherwise, a new instance is created, added to the collection, and returned.</returns>
    public T EnsureExtension<T>() where T : IInfrastructureExtension, new()
    {
        T extension = new T();
        return EnsureExtension(extension);
    }
}
