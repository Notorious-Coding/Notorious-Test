using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Database;

using Npgsql;

using Respawn;
using Respawn.Graph;

using System.Data.Common;

using Testcontainers.PostgreSql;


namespace NotoriousTest.PostgreSql;

public class PostgreContainerInfrastructure : PostgreContainerInfrastructure<string>
{
    public PostgreContainerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
    {
    }

    /// <summary>
    /// Gets the configuration key used to retrieve the default connection string.
    /// </summary>
    /// <remarks>Override this property in a derived class to specify a different configuration key if
    /// needed.</remarks>
    protected virtual string ConnectionStringKey => "ConnectionStrings:Default";
    public override async Task Initialize()
    {
        await base.Initialize();

        AddEntry(ConnectionStringKey, GetDatabaseConnectionString());
    }
}

public class PostgreContainerInfrastructure<TOutputConfiguration> : DockerDatabaseInfrastructure<PostgreSqlContainer, TOutputConfiguration>
{
    public PostgreContainerInfrastructure(Guid contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
    {
        Container = ConfigureSqlContainer(new PostgreSqlBuilder()).Build();
    }

    public override DbConnection GetConnection(string connectionString) => new NpgsqlConnection(connectionString);

    protected virtual PostgreSqlBuilder ConfigureSqlContainer(PostgreSqlBuilder builder) => builder;

    public override string GetDatabaseConnectionString()
    {
        var connectionString = new NpgsqlConnectionStringBuilder(Container.GetConnectionString());
        if (!string.IsNullOrEmpty(FullDbName))
        {
            connectionString.Database = FullDbName;
        }

        return connectionString.ToString();
    }

    protected override async Task CreateDatabase(DbConnection sqlConnection)
    {
        await using (DbCommand command = sqlConnection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE \"{FullDbName}\"";
            await command.ExecuteNonQueryAsync();
        }
    }


}
