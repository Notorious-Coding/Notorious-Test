namespace NotoriousTest.Configuration
{
    /// <summary>
    /// Make a class able to produce or consume configuration
    /// </summary>
    /// <typeparam name="T">Configuration type.</typeparam>
    public interface IConfigurationProducer<T> : IConfigurationProducer
    {
        new List<ConfigurationEntry<T>> OutputConfiguration { get; }

        List<ConfigurationEntry<object>> IConfigurationProducer.OutputConfiguration
            => OutputConfiguration
            .Select(ce => new ConfigurationEntry<object>(ce.Value, ce.Key))
            .ToList();

        /// <summary>
        /// Adds a configuration entry with the specified key and value to the output configuration.
        /// </summary>
        /// <param name="key">The unique key that identifies the configuration entry. Cannot be null or empty.</param>
        /// <param name="value">The value to associate with the specified key in the configuration entry.</param>
        void AddEntry(string key, T value);
    }

    public interface IConfigurationProducer
    {
        /// <summary>
        /// Configuration produced as Dictionary.
        /// </summary>
        List<ConfigurationEntry<object>> OutputConfiguration { get; }
    }
}
