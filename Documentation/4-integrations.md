## Integrations

- [TestContainers](#testcontainers)
- [SQL Server (Docker)](#sql-server-docker)
- [SQL Server (External)](#sql-server-external)
- [PostgreSQL (Docker)](#postgresql-docker)
- [PostgreSQL (External)](#postgresql-external)

### TestContainers

**NotoriousTest.TestContainers** is available as a separate package.

```
PM> Install-Package NotoriousTest.TestContainers
```

Or from the .NET CLI:

```
dotnet add package NotoriousTest.TestContainers
```

This package provides a base infrastructure that automatically starts and stops any TestContainers container at the beginning and end of the test campaign.

Inherit from `DockerContainerInfrastructure<TContainer, TOutputConfiguration>`:

```csharp
public class MyContainerInfrastructure : DockerContainerInfrastructure<MsSqlContainer, string>
{
    public MyContainerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        Container = new MsSqlBuilder().Build();
    }

    public override Task Reset() => Task.CompletedTask;
}
```

> ❗ Use `string` as the `TOutputConfiguration` type parameter if your infrastructure does not produce configuration, or use a custom type if it does.

---

### SQL Server (Docker)

**NotoriousTest.SqlServer** is available as a separate package.

```
PM> Install-Package NotoriousTest.SqlServer
```

Or from the .NET CLI:

```
dotnet add package NotoriousTest.SqlServer
```

#### Initialization

Inherit from `SqlServerContainerInfrastructure` to get a containerized SQL Server database that:

- **On initialization:** starts a SQL Server Docker container (via TestContainers) and creates a unique database.
- **On reset:** empties the database (via Respawn).
- **On destruction:** stops and removes the container.

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }
}
```

#### Test usage

```csharp
[Fact]
public async Task Test1()
{
    SqlServerInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<SqlServerInfrastructure>();
    await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
    {
        await sql.OpenAsync();
        // Arrange your database here.
    }
}
```

- **`GetDatabaseConnection()`** — returns a `DbConnection` (SqlConnection) pointing to the newly created database.
- **`GetDatabaseConnectionString()`** — returns the connection string for the newly created database.

#### Populating the database

Override `Initialize()` and call `base.Initialize()` first, then open the database connection and run your schema setup:

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize();

        await using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"CREATE TABLE Users (
            user_id INT IDENTITY(1,1) PRIMARY KEY,
            username NVARCHAR(50) NOT NULL UNIQUE
        );";
        await command.ExecuteNonQueryAsync();
    }
}
```

#### Generating configuration for the web app

Override the `ConnectionStringKey` property to control which appsettings key the connection string is published under.
By default, the key is `ConnectionStrings:Default`.

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";
}
```

The connection string is automatically published to `WebApplication` under this key.

#### Configure the container

Override `ConfigureSqlContainer` to customise the MsSql container:

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    protected override MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
    {
        return builder.WithPassword("NotoriousStrong(!)Password6");
    }
}
```

#### Configuring Respawn (table filtering)

Use the `TableToIgnore`, `TableToInclude`, `SchemasToInclude`, and `SchemasToExclude` init properties to control which tables Respawn resets:

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        TableToIgnore = ["Migrations"];
    }
}
```

#### Configure the database name prefix

By default, the database name is `NotoriousDb_{EnvironmentId}`.
Override `DbPrefix` to customise the prefix:

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        DbPrefix = "TestDb";
    }
}
```

---

### SQL Server (External)

For scenarios where a SQL Server instance is already running (e.g. a local or CI server), use `SqlServerInfrastructure` (from `NotoriousTest.SqlServer`).

Connection settings are loaded from `testsettings.json`:

```json
{
  "SqlServerInfrastructure": {
    "ConnectionString": "Server=localhost;User Id=sa;Password=yourPassword;TrustServerCertificate=True;"
  }
}
```

```csharp
public class SqlServerInfrastructure : SqlServerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry)
        : base(contextId, settingsProvider, logger, registry) { }

    protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";
}
```

The behavior is identical to the Docker version (creates/drops a unique database, resets via Respawn), but no container is started.

---

### PostgreSQL (Docker)

**NotoriousTest.PostgreSql** is available as a separate package.

```
PM> Install-Package NotoriousTest.PostgreSql
```

Or from the .NET CLI:

```
dotnet add package NotoriousTest.PostgreSql
```

#### Initialization

Inherit from `PostgreContainerInfrastructure` to get a containerized PostgreSQL database that:

- **On initialization:** starts a PostgreSQL Docker container (via TestContainers) and creates a unique database.
- **On reset:** empties the database (via Respawn).
- **On destruction:** stops and removes the container.

```csharp
public class PostgreSqlInfrastructure : PostgreContainerInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }
}
```

#### Test usage

```csharp
[Fact]
public async Task Test1()
{
    PostgreSqlInfrastructure postgreInfrastructure = CurrentEnvironment.GetInfrastructure<PostgreSqlInfrastructure>();
    await using (DbConnection sql = postgreInfrastructure.GetDatabaseConnection())
    {
        await sql.OpenAsync();
        // Arrange your database here.
    }
}
```

- **`GetDatabaseConnection()`** — returns a `DbConnection` (NpgsqlConnection) pointing to the newly created database.
- **`GetDatabaseConnectionString()`** — returns the connection string for the newly created database.

#### Populating the database

```csharp
public class PostgreSqlInfrastructure : PostgreContainerInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize();

        await using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"CREATE TABLE Users (
            user_id SERIAL PRIMARY KEY,
            username VARCHAR(50) NOT NULL UNIQUE
        );";
        await command.ExecuteNonQueryAsync();
    }
}
```

#### Generating configuration for the web app

Override `ConnectionStringKey` (default: `ConnectionStrings:Default`):

```csharp
public class PostgreSqlInfrastructure : PostgreContainerInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    protected override string ConnectionStringKey => "ConnectionStrings:PostgreSql";
}
```

#### Configure the container

```csharp
public class PostgreSqlInfrastructure : PostgreContainerInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    protected override PostgreSqlBuilder ConfigureSqlContainer(PostgreSqlBuilder builder)
    {
        return builder.WithPassword("NotoriousStrong(!)Password6");
    }
}
```

#### Configuring Respawn (table filtering)

```csharp
public class PostgreSqlInfrastructure : PostgreContainerInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        TableToIgnore = ["Migrations"];
    }
}
```

#### Configure the database name prefix

```csharp
public class PostgreSqlInfrastructure : PostgreContainerInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        DbPrefix = "TestDb";
    }
}
```

---

### PostgreSQL (External)

For scenarios where a PostgreSQL instance is already running, use `PostgreInfrastructure` (from `NotoriousTest.PostgreSql`).

Connection settings are loaded from `testsettings.json`:

```json
{
  "PostgreInfrastructure": {
    "ConnectionString": "Host=localhost;Username=postgres;Password=yourPassword;"
  }
}
```

```csharp
public class PostgreSqlInfrastructure : PostgreInfrastructure
{
    public PostgreSqlInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry)
        : base(contextId, settingsProvider, logger, registry) { }

    protected override string ConnectionStringKey => "ConnectionStrings:PostgreSql";
}
```
