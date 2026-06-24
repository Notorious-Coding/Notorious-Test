using Dapper;
using DoggyDog.UI.Context.Adapters;
using DoggyDog.UI.Context.Model;
using NotoriousTest.SqlLiteRegistry;
using Environment = DoggyDog.UI.Context.Model.Environment;

namespace DoggyDog.UI.Infrastructures;

internal class SqliteRegistryRepository(SqliteRegistryRepositoryConfiguration configuration)
    : NotoriousTest.SqlLiteRegistry.SqliteRegistryRepository(configuration), IRegistryRepository
{
    public async Task<IEnumerable<Process>> GetAll()
    {
        IEnumerable<InfrastructureRegistryEntryEntity> infrastructures =
            await Connection.QueryAsync<InfrastructureRegistryEntryEntity>($"SELECT * FROM {VERSIONED_TABLE_NAME}");

        return infrastructures.GroupBy(i => i.ProcessID)
            .Select(byProcess =>
            {
                return new Process(
                    byProcess.Key,
                    byProcess.First().ProcessName,
                    new Environment(Guid.Parse(byProcess.First().EnvironmentId),
                        byProcess.Select(p => p.ToInfrastructure())));
            });
    }
}
