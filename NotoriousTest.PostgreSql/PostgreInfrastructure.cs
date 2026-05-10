using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Settings;
using NotoriousTest.Database;
using NotoriousTest.Database.Settings;

using Npgsql;

using System.Data.Common;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.PostgreSql
{
    public class PostgreInfrastructure : PostgreInfrastructure<string, DatabaseSettings>
    {
        public PostgreInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry, EnvironmentSettings settings) : base(contextId, settingsProvider, logger, registry, settings)
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

    [Cleaner(typeof(PostgreInfrastructureCleaner))]
    public class PostgreInfrastructure<TOutputConfiguration, TSettings> : ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings> where TSettings : DatabaseSettings, new()
    {
        public PostgreInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry, EnvironmentSettings settings) : base(contextId, settingsProvider, logger, registry, settings) {}

        public override DbConnection GetConnection(string connectionString) => new NpgsqlConnection(connectionString);

        public override string GetDatabaseConnectionString()
        {
            var connectionString = new NpgsqlConnectionStringBuilder(Settings.ConnectionString);
            if (!string.IsNullOrEmpty(FullDbName))
            {
                connectionString.Database = FullDbName;
            }

            return connectionString.ToString();
        }

        protected override async Task CreateDatabase(DbConnection sqlConnection)
        {
            await using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE \"{FullDbName}\"";
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override async Task DropDatabase(DbConnection sqlConnection)
        {
            await using var connection = (NpgsqlConnection)GetDatabaseConnection();
            NpgsqlConnection.ClearPool(connection);

            await using (DbCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"DROP DATABASE \"{FullDbName}\"";
                await command.ExecuteNonQueryAsync();
            }
        }

    }
}
