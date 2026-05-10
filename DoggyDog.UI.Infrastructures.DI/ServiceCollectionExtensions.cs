using DoggyDog.UI.Context.Adapters;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Environments;
using NotoriousTest.SqlLiteRegistry;

namespace DoggyDog.UI.Infrastructures.DI;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection collection)
    {
        public IServiceCollection AddInfrastructures() =>
            collection
                .AddSingleton<IRegistryRepository, SqliteRegistryRepository>()
                .AddSingleton(new SqliteRegistryRepositoryConfiguration
                {
                    ConnectionString = new SqliteConnectionStringBuilder
                    {
                        DataSource = SqliteRegistryConfigurationDefaults.DefaultRegistryPath
                    }.ConnectionString
                });
    }
}
