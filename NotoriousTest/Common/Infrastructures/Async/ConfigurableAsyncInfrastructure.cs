using NotoriousTest.Common.Helpers;

namespace NotoriousTest.Common.Infrastructures.Async
{
    public abstract class ConfigurableAsyncInfrastructure<TConfiguration> : AsyncInfrastructure, IConfigurable<TConfiguration> where TConfiguration : class, new()
    {
        public TConfiguration Configuration { get; set; } = new();
        public ConfigurableAsyncInfrastructure(bool initialize = false) : base(initialize)
        {

        }

        public virtual Dictionary<string, string> GetConfigurationAsDictionary()
        {
            return Configuration.ToDictionary();
        }

    }

    public abstract class ConfigurableAsyncInfrastructure : ConfigurableAsyncInfrastructure<Dictionary<string, string>>
    {
        public ConfigurableAsyncInfrastructure(bool initialize = false) : base(initialize)
        {

        }

        public override Dictionary<string, string> GetConfigurationAsDictionary()
        {
            return Configuration;
        }
    }
}
