using Microsoft.Data.SqlClient;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Database;

namespace NotoriousTest.SqlServer
{
    public class SqlServerInfrastructureCleaner : IInfrastructureCleaner<DatabaseMetadata>
    {
        public async Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, DatabaseMetadata? metadata = null)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));
            await using var connection = new SqlConnection(metadata.ServerConnectionString);

            await connection.OpenAsync();
            SqlCommand? command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE [{metadata.DatabaseName}]";
            await command.ExecuteNonQueryAsync();
        }
    }
}
