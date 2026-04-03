

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

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

            collection.AddSingleton<ITestSettingsProvider, TestSettingsProvider>()
                .AddSingleton<SqliteRegistryProviderConfiguration>((_) => new()
                {
                    ConnectionString = builder.ConnectionString
                })
                .AddSingleton<IRegistry, SqliteRegistryProvider>()
                .AddSingleton<IWatchDog, DoggyDogWatchDog>();

        }
    }
}
