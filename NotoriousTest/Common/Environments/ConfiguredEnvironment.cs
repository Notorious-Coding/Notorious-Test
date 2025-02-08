using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Sync;

namespace NotoriousTest.Common.Environments
{
    public abstract class ConfiguredEnvironment : Environment
    {
        public Dictionary<string, string> EnvironmentConfiguration { get; set; } = new();

        public override void Initialize()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is ConfiguredInfrastructure consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                infra.Initialize();

                if (infra is ConfiguredInfrastructure producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }

    public abstract class ConfiguredEnvironment<TConfig> : Environment
        where TConfig: class, new()
    {
        public TConfig EnvironmentConfiguration { get; set; } = new();

        public override void Initialize()
        {
            foreach(Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if(infra is ConfiguredInfrastructure<TConfig> consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                infra.Initialize();

                if (infra is ConfiguredInfrastructure<TConfig> producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }
}
