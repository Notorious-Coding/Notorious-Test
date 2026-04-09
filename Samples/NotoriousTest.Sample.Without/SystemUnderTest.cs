using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

using Respawn;

using Testcontainers.MsSql;

using Xunit;
namespace NotoriousTest.Sample.Without
{
    public class SystemUnderTest : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public MsSqlContainer Container { get; set; }
        public HttpClient Client { get; set; }
        public string DatabaseName { get; set; }
        public Respawner Respawner { get; set; }

        public SystemUnderTest()
        {
            Container = new MsSqlBuilder().Build();
            DatabaseName = "TestDb_" + Guid.NewGuid();
        }

        public async ValueTask InitializeAsync()
        {
            await Container.StartAsync();
            await SetupDatabase();

            using var connection = GetDatabaseConnection();
            await connection.OpenAsync();
            Respawner = await Respawner.CreateAsync(connection);
            Client = CreateDefaultClient();
        }

        public async ValueTask DisposeAsync()
        {
            await Container.StopAsync();
            await base.DisposeAsync();
        }

        public string GetDatabaseConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(Container.GetConnectionString())
            {
                InitialCatalog = DatabaseName
            };

            return builder.ConnectionString;
        }

        public SqlConnection GetDatabaseConnection()
        {
            return new SqlConnection(GetDatabaseConnectionString());
        }

        private async Task SetupDatabase()
        {
            var connection = new SqlConnection(Container.GetConnectionString());

            await connection.OpenAsync();
            string dbsql = @$"CREATE DATABASE [{DatabaseName}]";
            using (SqlCommand command = connection.CreateCommand())
            {
                command.CommandText = dbsql;
                await command.ExecuteNonQueryAsync();
            }

            connection.ChangeDatabase(DatabaseName);

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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration((config) =>
            {
                config.AddInMemoryCollection([new("ConnectionStrings:SqlServer", GetDatabaseConnectionString())]);
            });
        }
    }
}
