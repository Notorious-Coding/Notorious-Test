using Microsoft.Data.Sqlite;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Database;

namespace NotoriousTest.PostgreSql
{
    public class SqliteInfrastructureCleaner : IInfrastructureCleaner<DatabaseMetadata>
    {
        public async Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, DatabaseMetadata metadata = null)
        {
            File.Delete(new SqliteConnectionStringBuilder(metadata.ServerConnectionString).DataSource);
        }
    }
}
