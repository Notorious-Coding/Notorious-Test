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
    public PostgreContainerInfrastructure(ContextId contextId, ITestLogger logger, IRegistryProvider registry) : base(contextId, logger, registry)
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
    public string[] SchemasToInclude { get; init; } = [];
    public string[] SchemasToExclude { get; init; } = [];

    public PostgreContainerInfrastructure(Guid contextId, ITestLogger logger, IRegistryProvider registry) : base(contextId, logger, registry)
    {
        Container = ConfigureSqlContainer(new PostgreSqlBuilder()).Build();
        EnsureExtension(new RespawnExtension(() => new RespawnerOptions()
        {
            TablesToIgnore = TableToIgnore.Select(tti => new Table(tti)).ToArray(),
            TablesToInclude = TableToInclude.Select(tti => new Table(tti)).ToArray(),
            SchemasToExclude = SchemasToExclude,
            SchemasToInclude = SchemasToInclude,
            DbAdapter = DbAdapter.Postgres
        }));
    }

    protected virtual PostgreSqlBuilder ConfigureSqlContainer(PostgreSqlBuilder builder)
    {
        return builder;
    }

    public override DbConnection GetDatabaseConnection()
    {
        return new NpgsqlConnection(GetDatabaseConnectionString());
    }

    public override DbConnection GetServerConnection()
    {
        return new NpgsqlConnection(GetServerConnectionString());
    }

    public override string GetDatabaseConnectionString()
    {
        NpgsqlConnectionStringBuilder connectionString = new NpgsqlConnectionStringBuilder(Container.GetConnectionString());
        if (!string.IsNullOrEmpty(FullDbName))
        {
            connectionString.Database = FullDbName;
        }

        return connectionString.ToString();
    }

    protected override async Task CreateDatabase(DbConnection sqlConnection)
    {
        using (DbCommand command = sqlConnection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE \"{FullDbName}\"";
            await command.ExecuteNonQueryAsync();
        }
    }
}