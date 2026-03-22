using NotoriousTest.Configuration;
using NotoriousTest.Extensions;

namespace NotoriousTest.Infrastructures;


public abstract class Infrastructure<TOutputConfiguration> : Infrastructure, IConfigurationProducer<TOutputConfiguration>
{
    public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration => OutputConfigurationExtension.OutputConfiguration;

    protected OutputConfigurationExtension<TOutputConfiguration> OutputConfigurationExtension { get; private set; }
    protected Infrastructure(ContextId contextId) : base(contextId)
    {
        OutputConfigurationExtension = EnsureExtension(new OutputConfigurationExtension<TOutputConfiguration>());
    }

    public void AddEntry(string key, TOutputConfiguration value) => OutputConfigurationExtension.AddEntry(key, value);
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

    ///<inheritdoc/>
    public ContextId ContextId { get; set; }

    private readonly List<IInfrastructureExtension> _extensions = new();


    public Infrastructure(ContextId contextId)
    {
        ContextId = contextId;
    }

    public abstract Task Initialize();
    public abstract Task Reset();
    public abstract Task Destroy();

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await DestroyAsync();
    }

    internal async Task InitializeAsync()
    {
        foreach (var extension in _extensions)
            await extension.OnBeforeInitialize(this);

        await Initialize();

        foreach (var extension in _extensions)
            await extension.OnAfterInitialize(this);
    }

    internal async Task ResetAsync()
    {
        foreach (var extension in _extensions)
            await extension.OnBeforeReset(this);

        await Reset();

        foreach (var extension in _extensions)
            await extension.OnAfterReset(this);
    }

    internal async Task DestroyAsync()
    {
        foreach (var extension in _extensions)
            await extension.OnBeforeDestroy(this);

        await Destroy();

        foreach (var extension in _extensions)
            await extension.OnAfterDestroy(this);
    }

    /// <summary>
    /// Ensures that an extension of the specified type is present in the collection, returning the existing instance if
    /// found or adding and returning the provided instance if not.
    /// </summary>
    /// <typeparam name="T">The type of the infrastructure extension to ensure. Must implement IInfrastructureExtension.</typeparam>
    /// <param name="extension">The extension instance to add if an existing instance of type T is not already present. Cannot be null.</param>
    /// <returns>The existing extension of type T if present; otherwise, the provided extension instance.</returns>
    protected T EnsureExtension<T>(T extension) where T : IInfrastructureExtension
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
    protected T EnsureExtension<T>() where T : IInfrastructureExtension, new()
    {
        var existing = _extensions.OfType<T>().FirstOrDefault();
        if (existing != null) return existing;

        T extension = new T();
        _extensions.Add(extension);
        return extension;
    }
}
