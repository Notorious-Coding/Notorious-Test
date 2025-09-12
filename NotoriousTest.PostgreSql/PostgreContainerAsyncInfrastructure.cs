using NotoriousTest.TestContainers;

using Npgsql;

using Respawn;

using Testcontainers.PostgreSql;


namespace NotoriousTest.PostgreSql;

public class PostgreContainerAsyncInfrastructure : DockerContainerAsyncInfrastructure<PostgreSqlContainer>
{
    public string DbName { get; init; } = "NotoriousDb";
    public RespawnerOptions? RespawnOptions { get; set; } = new RespawnerOptions
    {
        DbAdapter = DbAdapter.Postgres
    };

    protected string FullDbName;
    private Respawner _respawner;
    public PostgreContainerAsyncInfrastructure(bool initialize = false) : base(initialize)
    {
        Container = ConfigureSqlContainer(new PostgreSqlBuilder()).Build();
    }

    /// <summary>
    /// Returns a SQL Server connection connected to the current infrastructure's database.
    /// </summary>
    /// <returns>A SqlConnection instance connected to the current infrastructure's database.</returns>
    public NpgsqlConnection GetDatabaseConnection() => new NpgsqlConnection(GetConnectionString(FullDbName));

    /// <summary>
    /// Returns a SQL Server connection string pointing to the current infrastructure's database.
    /// </summary>
    /// <returns>A SqlConnection instance pointing to the current infrastructure's database.</returns>
    public string GetDatabaseConnectionString() => GetConnectionString(FullDbName);

    public override async Task Initialize()
    {
        await base.Initialize();

        FullDbName = $"{DbName}_{ContextId}";
        using (var connection = GetSqlConnection())
        {
            await connection.OpenAsync();
            await CreateDatabase(connection);
            await connection.ChangeDatabaseAsync(FullDbName);
            await PopulateDatabase(connection);
            _respawner = await Respawner.CreateAsync(connection, RespawnOptions);

        }
    }

    /// <summary>
    /// Called after the database is created. Override this method to populate the database with data.
    /// </summary>
    /// <param name="connection">A SqlConnection pointing on the newly created database</param>
    protected virtual Task PopulateDatabase(NpgsqlConnection connection)
    {
        return Task.CompletedTask;
    }

    public override async Task Reset()
    {
        using (var connection = GetSqlConnection())
        {
            await connection.OpenAsync();
            await connection.ChangeDatabaseAsync(FullDbName);
            await _respawner.ResetAsync(connection);
        }
    }

    protected virtual PostgreSqlBuilder ConfigureSqlContainer(PostgreSqlBuilder builder)
    {
        return builder;
    }

    private async Task CreateDatabase(NpgsqlConnection sqlConnection)
    {
        using (NpgsqlCommand command = sqlConnection.CreateCommand())
        {
            command.CommandText = $"CREATE DATABASE \"{FullDbName}\"";
            await command.ExecuteNonQueryAsync();
        }
    }

    private NpgsqlConnection GetSqlConnection() => new NpgsqlConnection(GetConnectionString());

    private string GetConnectionString(string? dbName = null)
    {
        NpgsqlConnectionStringBuilder connectionString = new NpgsqlConnectionStringBuilder(Container.GetConnectionString());

        if (!string.IsNullOrEmpty(dbName))
        {
            connectionString.Database = FullDbName;
        }

        return connectionString.ToString();
    }
}