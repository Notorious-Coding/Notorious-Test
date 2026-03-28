using Microsoft.Data.SqlClient;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Database;
using NotoriousTest.Database.Settings;

using Respawn;
using Respawn.Graph;

using System.Data.Common;

namespace NotoriousTest.SqlServer
{
    public class SqlServerInfrastructure : SqlServerInfrastructure<string, DatabaseSettings>
    {
        public SqlServerInfrastructure(ContextId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistryProvider registry) : base(contextId, settingsProvider, logger, registry)
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

    public class SqlServerInfrastructure<TOutputConfiguration, TSettings> : ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> where TSettings : DatabaseSettings, new()
    {
        public string[] SchemasToInclude { get; init; } = [];
        public string[] SchemasToExclude { get; init; } = [];

        public SqlServerInfrastructure(ContextId contextId, ITestSettingsProvider testSettingsProvider, ITestLogger logger, IRegistryProvider registry) : base(contextId, testSettingsProvider, logger, registry)
        {
            EnsureExtension(new RespawnExtension(() => new RespawnerOptions()
            {
                TablesToIgnore = TableToIgnore.Select(tti => new Table(tti)).ToArray(),
                TablesToInclude = TableToInclude.Select(tti => new Table(tti)).ToArray(),
                SchemasToExclude = SchemasToExclude,
                SchemasToInclude = SchemasToInclude,
                DbAdapter = DbAdapter.SqlServer
            }));
        }

        public override DbConnection GetDatabaseConnection()
        {
            return new SqlConnection(GetDatabaseConnectionString());
        }

        public override DbConnection GetServerConnection()
        {
            return new SqlConnection(GetServerConnectionString());
        }

        public override string GetDatabaseConnectionString()
        {
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(Settings.ConnectionString)
            {
                InitialCatalog = FullDbName
            };

            return connectionString.ToString();
        }

        protected override async Task CreateDatabase(DbConnection sqlConnection)
        {
            using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE [{FullDbName}]";
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override async Task DropDatabase(DbConnection sqlConnection)
        {
            using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"DROP DATABASE [{FullDbName}]";
                await command.ExecuteNonQueryAsync();
            }
        }


    }
}
