

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Settings;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.Runtime;
using NotoriousTest.SqlLiteRegistry;
using NotoriousTest.Watchdog;

namespace NotoriousTest.Environments
{
    public abstract class EnvironmentBase : Core.Environments.EnvironmentBase
    {
        protected override void ConfigureInfrastructureServices(IServiceCollection collection)
        {
            base.ConfigureInfrastructureServices(collection);

            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = EnvironmentBaseDefaults.DefaultRegistryConnectionString
            };

            var registryConfig = new SqliteRegistryProviderConfiguration()
            {
                ConnectionString = builder.ConnectionString
            };
            var settingsProvider = new TestSettingsProvider();
            collection.AddSingleton<ITestSettingsProvider>(settingsProvider)
                .AddSingleton<IRegistry>((_) => new SqliteRegistryProvider(registryConfig))
                .AddSingleton<IWatchDog>((services) => new DoggyDogWatchDog(registryConfig, services.GetRequiredService<ITestLogger>(), settingsProvider.Get<DoggyDogWatchdogConfiguration>("Watchdog") ?? new DoggyDogWatchdogConfiguration()))
                .AddSingleton<IRuntime, RuntimeConfigurationProvider>();

        }
    }
}
