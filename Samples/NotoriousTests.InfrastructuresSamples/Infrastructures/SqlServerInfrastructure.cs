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

        protected override MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            return builder.WithPassword("NotoriousStrong(!)Password6");
        }

        protected override async Task PopulateDatabase(SqlConnection connection)
        {
            // Play all your migrations script here, use DBUp or any other migration tool
            // You can also use EF Core to create the database schema, apply migrations, etc.
            await CreateTables(connection);
        }

        public override async Task Initialize()
        {
            await base.Initialize();
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
