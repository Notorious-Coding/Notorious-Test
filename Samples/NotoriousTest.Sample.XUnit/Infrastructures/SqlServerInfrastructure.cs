using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.SqlServer;

using System.Data.Common;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.Sample.XUnit.Infrastructures
{
    public class SqlServerInfrastructure : SqlServerContainerInfrastructure
    {
        public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry, EnvironmentSettings settings) : base(contextId, logger, registry, settings)
        {
        }

        protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";
        public override async Task Initialize()
        {
            await base.Initialize();
            // We can add the connection string to the configuration, it will provide a SqlConnection.

            using var connection = GetDatabaseConnection();
            await connection.OpenAsync();
            await CreateTables(connection);

        }

        private async Task CreateTables(DbConnection connection)
        {
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
    }
}
