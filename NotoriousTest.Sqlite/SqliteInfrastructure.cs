using Microsoft.Data.Sqlite;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Database;
using NotoriousTest.Database.Settings;

using Respawn;
using Respawn.Graph;

using System.Data.Common;

namespace NotoriousTest.Sqlite
{
    public class SqliteInfrastructure : SqliteInfrastructure<string, DatabaseSettings>
    {
        public SqliteInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry) : base(contextId, settingsProvider, logger, registry)
        {
        }

        /// <summary>
        /// Gets the configuration key used to retrieve the default connection string.
        /// </summary>
        /// <remarks>Override this property in a derived class to specify a different configuration key if
        /// needed.</remarks>
        protected virtual string ConnectionStringKey => "ConnectionStrings:Default";
        public override async Task Initialize()
        {
            await base.Initialize();

            AddEntry(ConnectionStringKey, GetDatabaseConnectionString());
        }
    }

    [Cleaner(typeof(SqliteInfrastructureCleaner))]
    public class SqliteInfrastructure<TOutputConfiguration, TSettings> : ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> where TSettings : DatabaseSettings, new()
    {
        private string _connectionString;
        public SqliteInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry) : base(contextId, settingsProvider, logger, registry)
        {
            EnsureExtension(new RespawnExtension(() => new RespawnerOptions()
            {
                TablesToIgnore = TableToIgnore.Select(tti => new Table(tti)).ToArray(),
                TablesToInclude = TableToInclude.Select(tti => new Table(tti)).ToArray(),
                DbAdapter = DbAdapter.Sqlite
            }));
        }

        public override DbConnection GetConnection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        public string GetPath()
        {
            return new SqliteConnectionStringBuilder(GetServerConnectionString()).DataSource;
        }
        public override string GetServerConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                var builder = new SqliteConnectionStringBuilder(Settings.ConnectionString);

                var directory = Path.GetDirectoryName(builder.DataSource) ?? string.Empty;
                var filename = Path.GetFileNameWithoutExtension(builder.DataSource);
                var extension = Path.GetExtension(builder.DataSource);

                builder.DataSource = Path.Combine(directory, $"{filename}_{FullDbName}{extension}");
                _connectionString = builder.ConnectionString;
            }

            return _connectionString;
        }

        public override string GetDatabaseConnectionString()
        {
            // For SQLite, the server connection string and the database connection string are the same, since SQLite is a file-based database.
            return GetServerConnectionString();
        }

        protected override async Task CreateDatabase(DbConnection sqliteConnection)
        {
            // Since the connection is already opened, nothing to do. The database file will be created automatically when the connection is opened if it does not exist.
        }

        protected override async Task DropDatabase(DbConnection sqlConnection)
        {
            await sqlConnection.CloseAsync();
            SqliteConnection.ClearAllPools();
            File.Delete(sqlConnection.DataSource);
        }

    }
}
