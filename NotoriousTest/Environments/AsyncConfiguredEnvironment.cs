using Microsoft.Extensions.Configuration;

using NotoriousTest.Configuration;
using NotoriousTest.Infrastructures.Async;

namespace NotoriousTest.Environments
{
    public abstract class AsyncConfiguredEnvironment : AsyncConfiguredEnvironment<Dictionary<string, string>>
    {
    }


    public abstract class AsyncConfiguredEnvironment<TOutputConfig> : AsyncEnvironment, IConfigurableEnvironment<TOutputConfig> where TOutputConfig : class, new()
    {
        /// <summary>
        /// Configuration produced by all the environment's infrastructures.
        /// </summary>
        public TOutputConfig OutputConfiguration { get; set; } = new TOutputConfig();

        public IConfiguration _environmentConfiguration;

        public async override Task Initialize()
        {
            _environmentConfiguration = LoadEnvironmentConfiguration();

            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (IsConfigurable(infra))
                {
                    Type? inputConfiguationType = GetInputConfigurationType(infra);
                    object infrastructureInputConfiguration = Activator.CreateInstance(inputConfiguationType);
                    _environmentConfiguration.GetSection(infra.GetType().Name).Bind(infrastructureInputConfiguration);

                    SetInputConfigurationValue(infra, infrastructureInputConfiguration);
                }

                await infra.Initialize();

                if (infra is IConfigurableInfrastructure<TOutputConfig> producer)
                {
                    OutputConfiguration = producer.OutputConfiguration;
                }
            }
        }


        /// <summary>
        /// Permet d'étendre la récupération de la configuration des tests.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns>Un builder représentant la configuration des tests.</returns>
        public virtual IConfigurationBuilder LoadConfiguration(IConfigurationBuilder builder)
        {
            string env = Environment.GetEnvironmentVariable("TESTS_ENVIRONMENT");

            return builder.AddJsonFile("testsettings.json", optional: true)
                .AddJsonFile($"testsettings.{env}.json", optional: true);
        }

        private IConfiguration LoadEnvironmentConfiguration()
        {
            return LoadConfiguration(new ConfigurationBuilder()).Build();
        }

        private bool IsConfigurable(AsyncInfrastructure infra)
        {
            return infra.GetType().GetInterfaces().Any(i => i.GetGenericTypeDefinition() == typeof(IConfigurableInfrastructure<,>));
        }
        private Type? GetInputConfigurationType(AsyncInfrastructure infra)
        {
            Type infraType = infra.GetType();
            Type? configurableInfrastructure = infraType.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConfigurableInfrastructure<,>));

            return configurableInfrastructure?.GetGenericArguments()[1];
        }

        private void SetInputConfigurationValue(AsyncInfrastructure infra, object configuration)
        {
            Type infraType = infra.GetType();
            infraType.GetProperty("InputConfiguration").SetValue(infra, configuration);
        }
    }
}
