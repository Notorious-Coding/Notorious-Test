using System.Text.Json;
using Dapper;
using DoggyDog.UI.Context.Adapters;
using DoggyDog.UI.Context.Model;
using NotoriousTest.SqlLiteRegistry;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Infrastructures;

internal class SqliteRegistryRepository(SqliteRegistryRepositoryConfiguration configuration)
    : NotoriousTest.SqlLiteRegistry.SqliteRegistryRepository(configuration), IRegistryRepository
{
    public async Task<IEnumerable<Environment>> GetAll()
    {
        IEnumerable<InfrastructureRegistryEntryEntity> infrastructures = await Connection.QueryAsync<InfrastructureRegistryEntryEntity>($"SELECT * FROM {TABLE_NAME}");

        return infrastructures
            .GroupBy((i) => i.EnvironmentId)
            .Select(gr =>
                new Environment(
                    Id: Guid.Parse(gr.Key),
                    Infrastructures: gr.Select(i => i.ToInfrastructure())
                )
            );
    }


}
