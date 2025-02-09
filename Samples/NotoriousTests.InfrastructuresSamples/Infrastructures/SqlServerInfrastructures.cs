using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.TestContainers.Infrastructures;
using Respawn;
using Testcontainers.MsSql;

namespace NotoriousTests.InfrastructuresSamples.Infrastructures
{
    internal class SqlServerInfrastructures : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer>
    {
        public Guid EnvironmentId { get; set; }
        private string DbName => $"TestDB_{EnvironmentId}";
        private Respawner DbRespawner { get; set; }
        protected override MsSqlContainer Container { get ; init; } = new MsSqlBuilder().Build();

        // This method is public so you can access a SQL Connection from your test, directly from the infrastructure.
        public SqlConnection GetConnection() => GetSqlConnection(DbName);

        /// <summary>
        /// This method is called at the beginning of the test campaign
        /// </summary>
        public override async Task Initialize()
        {
            await base.Initialize();
            var toto = Container.GetConnectionString();
            await using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await CreateDatabase(connection);

                connection.ChangeDatabase(DbName);
                // Play all your migrations script here, use DBUp or any other migration tool
                // You can also use EF Core to create the database schema, apply migrations, etc.
                await CreateTables(connection);
            }

            // Since this is a configuration producer, we can add the connection string to the configuration
            // It will then be consumed by IConfigurationConsumer infrastructures, like the WebApplication for example.
            // You can see how to retrieve this connection string in the Program.cs file.
            Configuration.Add("ConnectionStrings:SqlServer", GetConnectionString(DbName));
        }

        /// <summary>
        /// This method is called after each test. It is used to reset the database to an empty on known state.
        /// Here, i use respawn since it is one of the best solution to easily reset tables without destroying and recreating
        /// the whole database.
        /// </summary>
        public override async Task Reset()
        {
            await using (var connection = GetSqlConnection(DbName))
            {
                await connection.OpenAsync();
                DbRespawner = await Respawner.CreateAsync(connection);
                await DbRespawner.ResetAsync(connection);
                await connection.CloseAsync();
            }
        }


        private string GetConnectionString(string? dbName = null)
        {
            try
            {

            
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder(Container.GetConnectionString());
            if (!string.IsNullOrEmpty(dbName))
            {
                connectionString.InitialCatalog = DbName;
            }

            return connectionString.ToString();
            }catch(Exception e)
            {
                throw;
            }
        }

        private SqlConnection GetSqlConnection(string dbName = null) => new SqlConnection(GetConnectionString(dbName));

        private async Task CreateDatabase(SqlConnection sqlConnection)
        {
            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"CREATE DATABASE [{DbName}]";
                await command.ExecuteNonQueryAsync();
            }
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
