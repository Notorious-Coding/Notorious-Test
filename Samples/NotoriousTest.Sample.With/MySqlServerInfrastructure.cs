using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.SqlServer;

using System.Data.Common;

namespace NotoriousTest.Sample.With
{
    public class MySqlServerInfrastructure : SqlServerContainerInfrastructure
    {

        public MySqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
        }

        protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";

        public override async Task Initialize()
        {
            await base.Initialize();

            await CreateTables();
        }

        private async Task CreateTables()
        {
            using var connection = GetDatabaseConnection();
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
    }
}
