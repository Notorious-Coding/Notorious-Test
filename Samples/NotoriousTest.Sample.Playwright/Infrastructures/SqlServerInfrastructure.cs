using System.Data.Common;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.SqlServer;

namespace NotoriousTest.Sample.Playwright.Infrastructures;

/// <summary>
///     Database of the API, started in a SQL Server container.
/// </summary>
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId,
        logger, registry)
    {
    }

    // Key read by NotoriousTest.Sample.TestWebApp: Configuration.GetConnectionString("SqlServer").
    protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";

    public override async Task Initialize()
    {
        await base.Initialize();

        await using DbConnection connection = GetDatabaseConnection();
        await connection.OpenAsync();
        await CreateTables(connection);
    }

    private static async Task CreateTables(DbConnection connection)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = """
                              CREATE TABLE Users (
                                  user_id INT IDENTITY(1,1) PRIMARY KEY,
                                  username NVARCHAR(50) NOT NULL UNIQUE,
                                  email NVARCHAR(100) NOT NULL UNIQUE,
                                  password_hash NVARCHAR(255) NOT NULL,
                                  created_at DATETIME DEFAULT GETDATE()
                              );
                              """;
        await command.ExecuteNonQueryAsync();
    }
}
