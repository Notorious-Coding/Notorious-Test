using Microsoft.Data.SqlClient;

using NotoriousTest.Aspire;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

using System.Data.Common;
namespace NotoriousTest.Sample.AspireIntegrationTest
{
    public class SqlServerAspireInfrastructure : AspireDatabaseInfrastructure<SqlServerDatabaseResource, string, DockerMetadata>
    {
        protected string ContainerName => $"nt-sqlserver-container-{Id.ToString()}";
        public SqlServerAspireInfrastructure(IDistributedApplicationBuilder builder, EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(builder, contextId, logger, provider)
        {
        }

        public override DbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public override string GetDatabaseConnectionString()
        {
            return ((IResourceWithConnectionString)ResourceBuilder.Resource).GetConnectionStringAsync().Result;
        }

        public override string GetServerConnectionString()
        {
            return ResourceBuilder.Resource.Parent.GetConnectionStringAsync().Result;
        }

        public override async Task Initialize()
        {
            Metadata = new DockerMetadata()
            {
                ContainerName = ContainerName
            };

            AddEntry($"ConnectionStrings:{ResourceBuilder.Resource.Name}", GetDatabaseConnectionString());

            await CreateTables();
        }

        private async Task CreateTables()
        {
            using var connection = new SqlConnection(GetDatabaseConnectionString());
            await connection.OpenAsync();
            string sql = @"CREATE TABLE Users (
                                user_id INT IDENTITY(1,1) PRIMARY KEY,
                                username NVARCHAR(50) NOT NULL UNIQUE,
                                email NVARCHAR(100) NOT NULL UNIQUE,
                                password_hash NVARCHAR(255) NOT NULL,
                                created_at DATETIME DEFAULT GETDATE()
                            );
                            ";

            using (DbCommand command = connection.CreateCommand())
            {
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        protected override IResourceBuilder<SqlServerDatabaseResource> AddResource(IDistributedApplicationBuilder builder)
        {
            return builder
                .AddSqlServer("sql")
                .WithContainerName(ContainerName)
                .AddDatabase("SqlServer");
        }

        protected override Task CreateDatabase(DbConnection sqlConnection)
        {
            // Will be created by aspire;
            return Task.CompletedTask;
        }

        protected override Task DropDatabase(DbConnection sqlConnection)
        {
            // Will be cleaned with the docker container
            return Task.CompletedTask;
        }
    }
}
