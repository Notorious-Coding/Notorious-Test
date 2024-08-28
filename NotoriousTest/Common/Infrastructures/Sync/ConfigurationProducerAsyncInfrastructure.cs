using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.Common.Infrastructures.Sync
{
    public abstract class ConfigurationProducerInfrastructure<TConfig> : Infrastructure where TConfig: new()
    {
        public TConfig Configuration { get; protected set; }
        public ConfigurationProducerInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }

    public abstract class ConfigurationProducerInfrastructure : ConfigurationProducerInfrastructure<Dictionary<string, string>>
    {
        public ConfigurationProducerInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }
}
