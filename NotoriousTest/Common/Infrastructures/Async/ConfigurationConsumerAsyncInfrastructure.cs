using NotoriousTest.Common.Helpers;

namespace NotoriousTest.Common.Infrastructures.Async
{
    public abstract class ConfigurationConsumerAsyncInfrastructure : ConfigurationConsumerAsyncInfrastructure<Dictionary<string, string>>
    {
        public ConfigurationConsumerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }

    public abstract class ConfigurationConsumerAsyncInfrastructure<TConfig> : AsyncInfrastructure where TConfig : class
    {
        public TConfig Configuration { get; set; }

        public ConfigurationConsumerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }
}
