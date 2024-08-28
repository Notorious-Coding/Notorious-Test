using NotoriousTest.Common.Helpers;

namespace NotoriousTest.Common.Infrastructures.Sync
{
    public abstract class ConfigurationConsumerInfrastructure<TConfig> : Infrastructure
    {
        public TConfig Configuration { get; set; }

        public ConfigurationConsumerInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }

    public abstract class ConfigurationConsumerInfrastructure : ConfigurationConsumerInfrastructure<Dictionary<string, string>>
    {
        public ConfigurationConsumerInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }
}
