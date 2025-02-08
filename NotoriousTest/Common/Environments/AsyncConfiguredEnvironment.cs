using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.Common.Environments
{
    public abstract class AsyncConfiguredEnvironment : AsyncEnvironment
    {
        public Dictionary<string, string> Configuration { get; set; } = new();

        public async override Task Initialize()
        {
            await ConfigureEnvironmentAsync();

            foreach(AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if(infra is IConfigurationConsumer && infra is AsyncConfiguredInfrastructure consumer)
                {
                    consumer.Configuration = Configuration;

                }

                await infra.Initialize();

                if (infra is IConfigurationProducer && infra is AsyncConfiguredInfrastructure producer)
                {
                    Configuration = Configuration
                                        .Concat(producer.Configuration)
                                        .ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
    }

    public abstract class AsyncConfiguredEnvironment<TConfig> : AsyncEnvironment where TConfig : class, new()
    {
        public TConfig EnvironmentConfiguration { get; set; } = new();

        public async override Task Initialize()
        {
            await ConfigureEnvironmentAsync();

            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurationConsumer && infra is AsyncConfiguredInfrastructure<TConfig> consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                await infra.Initialize();
                
                if (infra is IConfigurationProducer && infra is AsyncConfiguredInfrastructure<TConfig> producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }
}
