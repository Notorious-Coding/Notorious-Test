namespace NotoriousTest.Core.Configuration;

/// <summary>
///     Make a class able to consume configuration
/// </summary>
public interface IConfigurationConsumer
{
    /// <summary>
    ///     Configuration consumed.
    /// </summary>
    List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
}
