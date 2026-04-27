using Microsoft.Data.Sqlite;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Database;

namespace NotoriousTest.Sqlite
{
    public class SqliteInfrastructureCleaner : IInfrastructureCleaner<DatabaseMetadata>
    {
        public async Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, DatabaseMetadata? metadata = null)
        {
            if (metadata != null)
                File.Delete(new SqliteConnectionStringBuilder(metadata.ServerConnectionString).DataSource);
        }
    }
}
