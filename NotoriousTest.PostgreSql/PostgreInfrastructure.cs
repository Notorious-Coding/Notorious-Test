using NotoriousTest.Database;
using NotoriousTest.Database.Settings;
using NotoriousTest.Settings;

using Npgsql;

using Respawn;
using Respawn.Graph;

using System.Data.Common;

namespace NotoriousTest.PostgreSql
{
    public class PostgreInfrastructure : PostgreInfrastructure<string, DatabaseSettings>
    {
        public PostgreInfrastructure(ContextId contextId, ITestSettingsProvider settingsProvider) : base(contextId, settingsProvider)
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

    public class PostgreInfrastructure<TOutputConfiguration, TSettings> : ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> where TSettings : DatabaseSettings, new()
    {
        public string[] SchemasToInclude { get; init; } = [];
        public string[] SchemasToExclude { get; init; } = [];

        public PostgreInfrastructure(ContextId contextId, ITestSettingsProvider settingsProvider) : base(contextId, settingsProvider)
        {
            EnsureExtension(new RespawnExtension(() => new RespawnerOptions()
            {
                TablesToIgnore = TableToIgnore.Select(tti => new Table(tti)).ToArray(),
                TablesToInclude = TableToInclude.Select(tti => new Table(tti)).ToArray(),
                SchemasToExclude = SchemasToExclude,
                SchemasToInclude = SchemasToInclude,
                DbAdapter = DbAdapter.Postgres
            }));
        }

        public override DbConnection GetDatabaseConnection()
        {
            return new NpgsqlConnection(GetDatabaseConnectionString());
        }

        public override DbConnection GetServerConnection()
        {
            return new NpgsqlConnection(GetServerConnectionString());
        }

        public override string GetDatabaseConnectionString()
        {
            NpgsqlConnectionStringBuilder connectionString = new NpgsqlConnectionStringBuilder(Settings.ConnectionString);
            if (!string.IsNullOrEmpty(FullDbName))
            {
                connectionString.Database = FullDbName;
            }

            return connectionString.ToString();
        }

        protected override async Task CreateDatabase(DbConnection sqlConnection)
        {
            using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE \"{FullDbName}\"";
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override async Task DropDatabase(DbConnection sqlConnection)
        {
            using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"DROP DATABASE \"{FullDbName}\"";
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
