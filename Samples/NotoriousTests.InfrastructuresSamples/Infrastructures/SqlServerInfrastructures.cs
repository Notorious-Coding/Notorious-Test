using Microsoft.Data.SqlClient;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;
using Respawn;

namespace NotoriousTests.InfrastructuresSamples.Infrastructures
{
    internal class SqlServerInfrastructures : AsyncConfiguredInfrastructure, IConfigurationProducer
    {
        public override int Order => 1;
        public Guid EnvironmentId { get; set; }
        private string DbName => $"TestDB_{EnvironmentId}";

        private Respawner DbRespawner { get; set; }
        public SqlConnection GetConnection() => GetSqlConnection(DbName);

        public override async Task Destroy()
        {
            await using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await CloseAllConnectionAndDestroyDatabase(connection);
            }
        }

        public override async Task Initialize()
        {
            await using (var connection = GetSqlConnection())
            {
                await connection.OpenAsync();
                await CreateDatabase(connection);

                connection.ChangeDatabase(DbName);
                // Play all your migrations script here, use DBUp or any other migration tool
                await CreateTables(connection);
            }

            Configuration.Add("ConnectionStrings:SqlServer", GetConnectionString(DbName));
        }

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
            SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder($"Server=localhost;User Id=sa;Password=Ttest123test;TrustServerCertificate=True");
            if (!string.IsNullOrEmpty(dbName))
            {
                connectionString.InitialCatalog = DbName;
            }

            return connectionString.ToString();
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
            // Apply your table creation scripts here
            var sql = File.ReadAllText("Tables.sql");

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task CloseAllConnectionAndDestroyDatabase(SqlConnection sqlConnection)
        {
            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $@"ALTER DATABASE [{DbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                                         DROP DATABASE [{DbName}];";
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
