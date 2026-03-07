using NotoriousTest.Configuration;
using NotoriousTest.Infrastructures.Sync;

namespace NotoriousTest.Environments
{
    public abstract class ConfiguredEnvironment : ConfiguredEnvironment<Dictionary<string, string>>
    {
    }

    public abstract class ConfiguredEnvironment<TConfig> : Environment, IConfigurableInfrastructure<TConfig>
        where TConfig : class, new()
    {
        public TConfig OutputConfiguration { get; set; } = new();

        public override void Initialize()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurableInfrastructure<TConfig> consumer)
                {
                    consumer.OutputConfiguration = OutputConfiguration;
                }

                infra.Initialize();

                if (infra is IConfigurableInfrastructure<TConfig> producer)
                {
                    OutputConfiguration = producer.OutputConfiguration;
                }
            }
        }
    }
}
