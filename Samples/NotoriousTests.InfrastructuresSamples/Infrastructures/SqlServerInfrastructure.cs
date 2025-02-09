using Microsoft.Data.SqlClient;
using NotoriousTest.SqlServer;
using NotoriousTest.TestContainers;
using Respawn;
using Respawn.Graph;
using Testcontainers.MsSql;

namespace NotoriousTests.InfrastructuresSamples.Infrastructures
{
    public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
            DbName = "TestDb";
            RespawnOptions = new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "MyMigrationTable" }
            };
        }

        /// <summary>
        /// This method is called at the beginning of the test campaign
        /// </summary>
        public override async Task Initialize()
        {
            await base.Initialize();

            using (var connection = GetDatabaseConnection())
            {
                await connection.OpenAsync();
                // Play all your migrations script here, use DBUp or any other migration tool
                // You can also use EF Core to create the database schema, apply migrations, etc.
                await CreateTables(connection);
            }
            // We can add the connection string to the configuration, it will provide a SqlConnection. 
            Configuration.Add("ConnectionStrings:SqlServer", GetDatabaseConnectionString());
        }

        private async Task CreateTables(SqlConnection connection)
        {
            var sql = File.ReadAllText("Tables.sql");

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
