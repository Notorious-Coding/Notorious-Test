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
            string sql = @"CREATE TABLE Users (
                                user_id INT IDENTITY(1,1) PRIMARY KEY,
                                username NVARCHAR(50) NOT NULL UNIQUE,
                                email NVARCHAR(100) NOT NULL UNIQUE,
                                password_hash NVARCHAR(255) NOT NULL,
                                created_at DATETIME DEFAULT GETDATE()
                            );
                            ";

            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
