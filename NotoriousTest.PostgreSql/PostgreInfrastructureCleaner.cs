using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Database;

using Npgsql;

namespace NotoriousTest.PostgreSql
{
    [InfrastructureCleaner(typeof(PostgreInfrastructure<,>))]
    public class PostgreInfrastructureCleaner : IInfrastructureCleaner<DatabaseMetadata>
    {
        public async Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, DatabaseMetadata metadata = null)
        {
            using var connection = new NpgsqlConnection(metadata.ServerConnectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE \"{metadata.DatabaseName}\"";
            await command.ExecuteNonQueryAsync();
        }
    }
}
