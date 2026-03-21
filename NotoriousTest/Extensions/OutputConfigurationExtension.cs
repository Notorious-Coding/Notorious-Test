using NotoriousTest.Configuration;

namespace NotoriousTest.Extensions;

// Extension OutputConfiguration
public class OutputConfigurationExtension<TOutputConfiguration> : IInfrastructureExtension
{
    public List<ConfigurationEntry<TOutputConfiguration>> OutputConfiguration { get; } = new();

    public void AddEntry(string key, TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, key));

    public void AddEntry(TOutputConfiguration value)
        => OutputConfiguration.Add(new ConfigurationEntry<TOutputConfiguration>(value, value!.GetType().Name));
}
