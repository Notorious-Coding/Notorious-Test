using Dapper;

using Microsoft.Data.Sqlite;

using NotoriousTest.Core.Registry;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace NotoriousTest.SqlLiteRegistry
{
    public partial class SqliteRegistryProvider : IRegistry, IAsyncDisposable
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
            await Connection.ExecuteAsync(ENSURE_REGISTRY);
        }

        public async Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry)
        {

            InfrastructureRegistryEntryEntity entity = await Connection.QuerySingleAsync<InfrastructureRegistryEntryEntity>(REGISTER_INFRASTRUCTURE, InfrastructureRegistryEntryEntity.FromDomain(entry));

            return entity.ToDomain();
        }

        public async ValueTask DisposeAsync()
        {
            await Connection.DisposeAsync();
        }

        public async Task<bool> Remove(Guid id)
        {
            int deletedRows = await Connection.ExecuteAsync(REMOVE_INFRASTRUCTURE, new { InfrastructureId = id.ToString() });
            return deletedRows > 0;
        }

        public async Task<IEnumerable<InfrastuctureRegistryEntry>> GetByProcessId(int processId)
        {
            IEnumerable<InfrastructureRegistryEntryEntity> entries = await Connection.QueryAsync<InfrastructureRegistryEntryEntity>(GET_BY_PROCESS_ID, new { ProcessID = processId });
            return entries.Select(entry => entry.ToDomain());
        }

        public void Empty()
        {

        }
    }
}
