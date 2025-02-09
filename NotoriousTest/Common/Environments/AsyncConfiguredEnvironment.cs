using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.Common.Environments
{
    public abstract class AsyncConfiguredEnvironment : AsyncEnvironment
    {
        public Dictionary<string, string> EnvironmentConfiguration { get; set; } = new();

        public async override Task Initialize()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is AsyncConfiguredInfrastructure consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                await infra.Initialize();

                if (infra is AsyncConfiguredInfrastructure producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }

    public abstract class AsyncConfiguredEnvironment<TConfig> : AsyncEnvironment where TConfig : class, new()
    {
        public TConfig EnvironmentConfiguration { get; set; } = new();

        public async override Task Initialize()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is AsyncConfiguredInfrastructure<TConfig> consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                await infra.Initialize();
                
                if (infra is AsyncConfiguredInfrastructure<TConfig> producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }
}
