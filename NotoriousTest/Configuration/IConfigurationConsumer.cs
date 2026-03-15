namespace NotoriousTest.Configuration
{
    public interface IConfigurationConsumer
    {
        List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
    }
}
