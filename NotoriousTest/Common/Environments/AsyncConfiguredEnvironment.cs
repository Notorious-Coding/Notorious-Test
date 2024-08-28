using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Web.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Environments
{
    public abstract class AsyncConfiguredEnvironment : AsyncEnvironment
    {
        public Dictionary<string, string> Configuration { get; set; } = new();

        public async override Task InitializeEnvironmentAsync()
        {
            await ConfigureEnvironmentAsync();

            foreach(AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if(infra is ConfigurationConsumerAsyncInfrastructure configurationConsumerAsyncInfrastructure)
                {
                    (configurationConsumerAsyncInfrastructure).Configuration = Configuration;

                }

                await infra.Initialize();

                if (infra is ConfigurationProducerAsyncInfrastructure configurationProducerAsyncInfrastructure)
                {
                    Configuration = Configuration
                                        .Concat(configurationProducerAsyncInfrastructure.Configuration)
                                        .ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
    }

    public abstract class AsyncConfiguredEnvironment<TConfig> : AsyncEnvironment where TConfig : class, new()
    {
        public TConfig EnvironmentConfiguration { get; set; } = new();

        public async override Task InitializeEnvironmentAsync()
        {
            await ConfigureEnvironmentAsync();

            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is ConfigurationConsumerAsyncInfrastructure<TConfig> consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                await infra.Initialize();
                
                if (infra is ConfigurationProducerAsyncInfrastructure<TConfig> producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }
}
