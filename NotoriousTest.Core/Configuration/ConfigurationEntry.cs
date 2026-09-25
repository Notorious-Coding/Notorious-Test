namespace NotoriousTest.Core.Configuration;

/// <summary>
///     A reprensentation of a piece of configuration outputed by infrastrucutre and consumed by others.
/// </summary>
/// <typeparam name="T">Object type of the configuration.</typeparam>
public record ConfigurationEntry<T>
{
    /// <summary>
    ///     Creates a new configuration entry.
    /// </summary>
    /// <param name="value">Value</param>
    /// <param name="section">Key, default to the type name.</param>
    /// <exception cref="ArgumentNullException">Threw if no value is provided.</exception>
    public ConfigurationEntry(T value, string? section = null)
    {
        Value = value;
        Key = section ?? value.GetType().Name ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Value of the configuration.
    /// </summary>
    public T Value { get; set; }

    /// <summary>
    ///     Key of the configuration.
    /// </summary>
    public string Key { get; set; }
}
