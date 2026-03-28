using Dapper;

using Microsoft.Data.Sqlite;

using NotoriousTest.Core.Registry;

using System;
using System.IO;
using System.Threading.Tasks;
namespace NotoriousTest.SqlLiteRegistry
{
    public class SqliteRegistryProvider : IRegistry, IAsyncDisposable
    {
        private static string RegistryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "notorioustest");
        private const string RegistryFileName = "doggydog-registry.db";
        protected SqliteConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _connection = new SqliteConnection(ConnectionString.ToString());
                }

                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                _connection.Execute("PRAGMA SYNCHRONOUS=NORMAL;PRAGMA JOURNAL_MODE=WAL;");

                return _connection;
            }
        }

        private SqliteConnection _connection;
        private SqliteConnectionStringBuilder ConnectionString => new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(RegistryFolder, RegistryFileName)
        };

        public async Task Ensure()
        {
            if (!Directory.Exists(RegistryFolder))
            {
                Directory.CreateDirectory(RegistryFolder);
            }

            // WAL mode to handle multiple write
            // SYNCHRONOUS NORMAL to flush only when 
            await Connection.ExecuteAsync(SqliteRegistryProviderQueries.ENSURE_REGISTRY);
        }

        public async Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry)
        {

            InfrastructureRegistryEntryEntity entity = await Connection.QuerySingleAsync<InfrastructureRegistryEntryEntity>(SqliteRegistryProviderQueries.REGISTER_INFRASTRUCTURE, InfrastructureRegistryEntryEntity.FromDomain(entry));

            return entity.ToDomain();
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        public async Task<bool> Remove(Guid id)
        {
            int deletedRows = await Connection.ExecuteAsync(SqliteRegistryProviderQueries.REMOVE_INFRASTRUCTURE, new { InfrastructureId = id.ToString() });
            return deletedRows > 0;
        }
    }
}
