using NotoriousTest.Configuration;

namespace NotoriousTest.Extensions;

/// <summary>
/// Infrastructure extension that collects typed configuration entries to expose as output configuration.
/// </summary>
/// <typeparam name="TOutputConfiguration">The type of configuration value produced.</typeparam>
public class OutputConfigurationExtension<TOutputConfiguration> : IInfrastructureExtension
{
    /// <summary>Gets the list of typed configuration entries collected by this extension.</summary>
    public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration { get; } = new();

    /// <summary>Adds a configuration entry with the specified key and value.</summary>
    /// <param name="key">The configuration key.</param>
    /// <param name="value">The configuration value.</param>
    public void AddEntry(string key, TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, key));

    /// <summary>Adds a configuration entry using the type name of the value as the key.</summary>
    /// <param name="value">The configuration value.</param>
    public void AddEntry(TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, value!.GetType().Name));
}
