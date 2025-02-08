using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Sync;

namespace NotoriousTest.Common.Environments
{
    public abstract class ConfiguredEnvironment : Environment
    {
        protected Dictionary<string, string> Configuration { get; set; } = new();

        public override void Initialize()
        {
            foreach (Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                // Si l'infrastructure consomme la configuration 
                // (exemple: WebApp, on lui donne la configuration de l'environnement avant de l'initialiser
                if (infra is IConfigurationConsumer && infra is ConfiguredInfrastructure consumer)
                {
                    consumer.Configuration = Configuration;
                }

                infra.Initialize();

                // Si l'infrastructure produit de la configuration, on l'initialize avant, puis on ajoute la configuration produite a la configuration de l'environnement.
                if (infra is IConfigurationProducer && infra is ConfiguredInfrastructure producer)
                {
                    Configuration = Configuration
                                        .Concat(producer.Configuration)
                                        .ToDictionary(x => x.Key, x => x.Value);
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
                if(infra is IConfigurationConsumer && infra is ConfiguredInfrastructure<TConfig> consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                infra.Initialize();

                if (infra is IConfigurationProducer && infra is ConfiguredInfrastructure<TConfig> producer)
                {
                    EnvironmentConfiguration = producer.Configuration;
                }
            }
        }
    }
}
