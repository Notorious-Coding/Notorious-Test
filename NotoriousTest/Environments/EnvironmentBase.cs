

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.SqlLiteRegistry;
using NotoriousTest.Watchdog;

namespace NotoriousTest.Environments
{
    public abstract class EnvironmentBase : Core.Environments.EnvironmentBase
    {

        public override void ConfigureInfrastructureServices(IServiceCollection collection)
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

            collection.AddSingleton<ITestSettingsProvider, TestSettingsProvider>()
                .AddSingleton<IRegistry>((_) => new SqliteRegistryProvider(registryConfig))
                .AddSingleton<IWatchDog>((_) => new DoggyDogWatchDog(registryConfig, _.GetRequiredService<ITestLogger>()));

        }
    }
}
