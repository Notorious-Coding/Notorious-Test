using Dapper;

using Microsoft.Data.Sqlite;

using NotoriousTest.Core.Registry;
using NotoriousTest.SqlLiteRegistry.TypeHandler;

using System;
using System.IO;
using System.Threading.Tasks;
namespace NotoriousTest.SqlLiteRegistry
{
    public class SqliteRegistryProvider : IRegistryProvider, IAsyncDisposable
    {
        private static string RegistryFolder = Path.Combine(Path.GetTempPath(), "notorioustest");
        private const string RegistryFileName = "doggydog-registry.db";
        private SqliteConnection _connection;
        private SqliteConnectionStringBuilder ConnectionString => new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(RegistryFolder, RegistryFileName)
        };

        public SqliteRegistryProvider()
        {
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            _connection = new SqliteConnection(ConnectionString.ToString());
            _connection.Open();

            // WAL mode to handle multiple write
            // SYNCHRONOUS NORMAL to flush only when 
            _connection.Execute("PRAGMA SYNCHRONOUS=NORMAL;PRAGMA JOURNAL_MODE=WAL;");
        }

        public async Task Ensure()
        {
            if (!Directory.Exists(RegistryFolder))
            {
                Directory.CreateDirectory(RegistryFolder);
            }


            await _connection.ExecuteAsync(SqliteRegistryProviderQueries.CREATE_REGISTRY);
        }

        public async Task<InfrastuctureRegistryEntry> Register(InfrastuctureRegistryEntry entry)
        {
            return await _connection.QuerySingleAsync<InfrastuctureRegistryEntry>(SqliteRegistryProviderQueries.REGISTER_INFRASTRUCTURE, entry);
        }

        public async ValueTask DisposeAsync()
        {
            await _connection.DisposeAsync();
        }
    }
}
