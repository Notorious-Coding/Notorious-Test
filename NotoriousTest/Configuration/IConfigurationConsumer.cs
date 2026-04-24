namespace NotoriousTest.Configuration
{
    /// <summary>
    /// Indicates that a class can consume configuration entries produced by other infrastructures.
    /// </summary>
    public interface IConfigurationConsumer
    {
        /// <summary>Gets or sets the list of configuration entries consumed by this instance.</summary>
        List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
    }
}
