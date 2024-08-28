using NotoriousTest.Common.Helpers;

namespace NotoriousTest.Common.Infrastructures.Async
{
    public abstract class ConfigurationProducerAsyncInfrastructure<TConfig> : AsyncInfrastructure where TConfig: new()
    {
        public TConfig Configuration { get; protected set; } = new();
        public ConfigurationProducerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }

    public abstract class ConfigurationProducerAsyncInfrastructure : ConfigurationProducerAsyncInfrastructure<Dictionary<string, string>>
    {
        public ConfigurationProducerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {

        }
    }
}
