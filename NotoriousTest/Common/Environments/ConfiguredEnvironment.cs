using NotoriousTest.Common.Configuration;
using NotoriousTest.Common.Infrastructures;

namespace NotoriousTest.Common.Environments
{
    public abstract class ConfiguredEnvironment : ConfiguredEnvironment<Dictionary<string, string>>
    {
    }

    public abstract class ConfiguredEnvironment<TConfig> : Environment, IConfigurable<TConfig> where TConfig : class, new()
    {
        public TConfig Configuration { get; set; } = new();

        public async override Task Initialize()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
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
