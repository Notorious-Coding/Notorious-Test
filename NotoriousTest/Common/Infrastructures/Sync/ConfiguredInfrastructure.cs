namespace NotoriousTest.Common.Infrastructures.Sync
{
    /// <summary>
    /// ConfiguredInfrastructure is a base class to define a test infrastructure with configuration.
    /// </summary>
    /// <typeparam name="TConfig">Configuration type</typeparam>
    public abstract class ConfiguredInfrastructure<TConfig> : Infrastructure where TConfig: new()
    {
        public TConfig Configuration { get; set; }
        public ConfiguredInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }

    /// <summary>
    /// ConfiguredInfrastructure is a base class to define a test infrastructure with configuration. Configuration is a dictionary of strings.
    /// </summary>
    public abstract class ConfiguredInfrastructure : ConfiguredInfrastructure<Dictionary<string, string>>
    {
        public ConfiguredInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }
}
