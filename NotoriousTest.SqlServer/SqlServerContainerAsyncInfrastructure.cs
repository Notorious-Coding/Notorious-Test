using Microsoft.Data.SqlClient;
using NotoriousTest.TestContainers;
using Respawn;
using System.Xml.Linq;
using Testcontainers.MsSql;

namespace NotoriousTest.SqlServer
{
    public class SqlServerContainerAsyncInfrastructure<TConfig> : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer, TConfig> where TConfig: new()
    {
        public string DbName { get; init; } = "NotoriousDb";
        public RespawnerOptions? RespawnOptions { get; set; } = null;
        protected string FullDbName;
        private Respawner _respawner;
        public SqlServerContainerAsyncInfrastructure(bool initialize = false) : base(initialize)
        {
            Container = ConfigureSqlContainer(new MsSqlBuilder()).Build();
        }

        /// <summary>
        /// Returns a SQL Server connection connected to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
        public SqlConnection GetDatabaseConnection() => new SqlConnection(GetConnectionString(FullDbName));

        /// <summary>
        /// Returns a SQL Server connection string pointing to the current infrastructure's database.
        /// </summary>
        /// <returns>A SqlConnection instance pointing to the current infrastructure's database.</returns>
        public string GetDatabaseConnectionString() => GetConnectionString(FullDbName);

        public override async Task Initialize()
        {
            await base.Initialize();

            FullDbName = $"{DbName}_{ContextId}";
            using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await CreateDatabase(connection);
                _respawner = await Respawner.CreateAsync(connection, RespawnOptions);

                await connection.ChangeDatabaseAsync(FullDbName);
                await PopulateDatabase(connection);
            }
        }

        /// <summary>
        /// Called after the database is created. Override this method to populate the database with data.
        /// </summary>
        /// <param name="connection">A SqlConnection pointing on the newly created database</param>
        protected virtual Task PopulateDatabase(SqlConnection connection)
        {
            return Task.CompletedTask;
        }

        public override async Task Reset()
        {
            using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await _respawner.ResetAsync(connection);
            }
        }

        protected virtual MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            return builder;
        }

        private async Task CreateDatabase(SqlConnection sqlConnection)
        {
            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE [{FullDbName}]";
                await command.ExecuteNonQueryAsync();
            }
        }
        private SqlConnection GetSqlConnection() => new SqlConnection(GetConnectionString());
        private string GetConnectionString(string? dbName = null)
        {
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(Container.GetConnectionString());
            if (!string.IsNullOrEmpty(dbName))
            {
                connectionString.InitialCatalog = FullDbName;
            }

            return connectionString.ToString();
        }
    }
    public class SqlServerContainerAsyncInfrastructure : SqlServerContainerAsyncInfrastructure<Dictionary<string, string>>
    {
    }
}
