using NotoriousTest.Common.Helpers;
using NotoriousTest.Common.Infrastructures.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.Common.Environments
{
    public abstract class ConfiguredEnvironment : Environment
    {
        protected Dictionary<string, string> Configuration { get; set; } = new();

        public override void InitializeEnvironment()
        {
            ConfigureEnvironment();

            foreach (Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                // Si l'infrastructure consomme la configuration 
                // (exemple: WebApp, on lui donne la configuration de l'environnement avant de l'initialiser
                if (infra is ConfigurationConsumerInfrastructure configurationConsumerAsyncInfrastructure)
                {
                    (configurationConsumerAsyncInfrastructure).Configuration = Configuration;
                }

                infra.Initialize();

                // Si l'infrastructure produit de la configuration, on l'initialize avant, puis on ajoute la configuration produite a la configuration de l'environnement.
                if (infra is ConfigurationProducerInfrastructure configurationProducerAsyncInfrastructure)
                {
                    Configuration = Configuration
                                        .Concat(configurationProducerAsyncInfrastructure.Configuration)
                                        .ToDictionary(x => x.Key, x => x.Value);
                }
            }
        }
    }

    public abstract class ConfiguredEnvironment<TConfig> : Environment
        where TConfig: class, new()
    {
        public TConfig EnvironmentConfiguration { get; set; } = new();

        public override void InitializeEnvironment()
        {
            ConfigureEnvironment();

            foreach(Infrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if(infra is ConfigurationConsumerInfrastructure<TConfig> consumer)
                {
                    consumer.Configuration = EnvironmentConfiguration;
                }

                infra.Initialize();

                if (infra is ConfigurationProducerInfrastructure<TConfig> producer)
                {
                    EnvironmentConfiguration = producer.Configuration;

                }
            }
        }
    }
}
