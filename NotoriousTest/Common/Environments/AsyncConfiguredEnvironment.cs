using NotoriousTest.Common.Configuration;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;

namespace NotoriousTest.Common.Environments
{
    public abstract class AsyncConfiguredEnvironment : AsyncConfiguredEnvironment<Dictionary<string, string>>
    {
    }

    public abstract class AsyncConfiguredEnvironment<TConfig> : AsyncEnvironment, IConfigurable<TConfig> where TConfig : class, new()
    {
        public TConfig Configuration { get; set; } = new();

        public async override Task Initialize()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurable<TConfig> consumer)
                {
                    consumer.Configuration = Configuration;
                }

                await infra.Initialize();
                
                if (infra is IConfigurable<TConfig> producer)
                {
                    Configuration = producer.Configuration;
                }
            }
        }
    }
}
