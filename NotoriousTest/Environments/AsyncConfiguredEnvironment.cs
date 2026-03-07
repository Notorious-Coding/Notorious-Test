using NotoriousTest.Configuration;
using NotoriousTest.Infrastructures.Async;

using System.Text.Json;

namespace NotoriousTest.Environments
{
    public abstract class AsyncConfiguredEnvironment : AsyncConfiguredEnvironment<Dictionary<string, string>>
    {
    }

    public abstract class AsyncConfiguredEnvironment<TOutputConfig> : AsyncEnvironment, IConfigurableEnvironment<TOutputConfig> where TOutputConfig : class, new()
    {
        public TOutputConfig OutputConfiguration { get; set; } = new TOutputConfig();

        public async override Task Initialize()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurableInfrastructure<TOutputConfig> consumer)
                {
                    consumer.OutputConfiguration = OutputConfiguration;
                }

                await infra.Initialize();

                if (infra is IConfigurableInfrastructure<TOutputConfig> producer)
                {
                    OutputConfiguration = producer.OutputConfiguration;
                }
            }
        }
    }

    public abstract class AsyncConfiguredEnvironment<TOutputConfig, TInputConfig> : AsyncEnvironment, IConfigurableEnvironment<TOutputConfig, TInputConfig> where TOutputConfig : class, new() where TInputConfig : class, new()
    {
        /// <summary>
        /// Configuration produced by all the environment's infrastructures.
        /// </summary>
        public TOutputConfig OutputConfiguration { get; set; } = new TOutputConfig();

        /// <summary>
        /// Configuration provided by config file.
        /// </summary>
        public TInputConfig InputConfiguration
        {
            get
            {
                var path = Path.Combine(AppContext.BaseDirectory, "tests.appsettings.json");

                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    return JsonSerializer.Deserialize<TInputConfig>(json) ?? new TInputConfig();
                }
                else
                {
                    return new TInputConfig();
                }
            }
        }

        public async override Task Initialize()
        {
            foreach (AsyncInfrastructure infra in Infrastructures.OrderBy(i => i.Order))
            {
                if (infra is IConfigurableInfrastructure<TOutputConfig, TInputConfig> consumer)
                {
                    consumer.InputConfiguration = InputConfiguration;
                }

                await infra.Initialize();

                if (infra is IConfigurableInfrastructure<TOutputConfig, TInputConfig> producer)
                {
                    OutputConfiguration = producer.OutputConfiguration;
                }
            }
        }
    }
}
