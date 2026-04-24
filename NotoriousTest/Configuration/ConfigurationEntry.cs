namespace NotoriousTest.Configuration
{
    /// <summary>
    /// Represents a configuration entry with a value and an optional section key.
    /// </summary>
    /// <typeparam name="T">The type of the configuration value.</typeparam>
    public record ConfigurationEntry<T>
    {
        /// <summary>Gets or sets the configuration value.</summary>
        public T Value { get; set; }

        /// <summary>Gets or sets the configuration key, defaulting to the type name of the value.</summary>
        public string Key { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ConfigurationEntry{T}"/> with the specified value and optional section.
        /// </summary>
        /// <param name="value">The configuration value.</param>
        /// <param name="section">The section key. If null, the type name of <paramref name="value"/> is used.</param>
        public ConfigurationEntry(T value, string? section = null)
        {
            Value = value;
            Key = section ?? value?.GetType().Name ?? throw new ArgumentNullException(nameof(value));
        }
    };
}
