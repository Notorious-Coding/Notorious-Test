# 🔌 Integrations

NotoriousTest provides ready-to-use infrastructure packages for the most common external dependencies.

## Summary

- [Web — `NotoriousTest.Web`](#-web--notorioustestweb)
- [Generic Docker Containers — `NotoriousTest.TestContainers`](#-generic-docker-containers--notorioustesttestcontainers)
- [SQL Server — `NotoriousTest.SqlServer`](#-sql-server--notorioustestsqlserver)
  - [Docker variant](#docker-variant)
  - [External variant](#external-variant)
- [PostgreSQL — `NotoriousTest.PostgreSql`](#-postgresql--notorioustestpostgresql)
  - [Docker variant](#docker-variant-1)
  - [External variant](#external-variant-1)
- [SQLite — `NotoriousTest.Sqlite`](#-sqlite--notorioustestsqlite)

---

## Available Packages

| Package | Description |
|---------|-------------|
| `NotoriousTest.Web` | ASP.NET Core web application factory with automatic configuration injection |
| `NotoriousTest.TestContainers` | Generic base class for any Testcontainers-powered Docker container |
| `NotoriousTest.Database` | Abstract base classes for database infrastructures (Docker & external) |
| `NotoriousTest.SqlServer` | SQL Server — Docker container or external server |
| `NotoriousTest.PostgreSql` | PostgreSQL — Docker container or external server |
| `NotoriousTest.Sqlite` | SQLite — file-based database |

---

## 🌐 Web — `NotoriousTest.Web`

Provides infrastructure for testing ASP.NET Core applications. The web application is automatically configured with all configuration entries produced by other infrastructures.

### Installation

```
dotnet add package NotoriousTest.Web
```

### Defining a Web Application

Create a class inheriting from `WebApplication<TEntryPoint>`, where `TEntryPoint` is your application's `Program` class:

```csharp
public class TestWebApplication : WebApplication<Program>
{
    // Override WebApplicationFactory methods if needed
    // e.g. ConfigureWebHost, CreateHost, etc.
}
```

### Registering in an Environment

Use the `AddWebApplication<T>()` extension method from `NotoriousTest.Web`:

```csharp
public class MyTestEnvironment : NotoriousTest.XUnit.Environment
{
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<MyDatabaseInfrastructure>(); // produces configuration
        this.AddWebApplication<TestWebApplication>();   // consumes it automatically
    }
}
```

📌 **Key Points:**
- `WebApplicationInfrastructure` always initializes with `Order = 999` — after all other infrastructures.
- It implements `IConfigurationConsumer`: all `ConfigurationEntry` objects produced by other infrastructures are injected into the app's `IConfiguration` at startup.
- Nested objects are flattened to colon-separated appsettings keys: `"ConnectionStrings:MyDb"`.

### Using in Tests

```csharp
HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
HttpResponseMessage response = await client.GetAsync("api/health");
Assert.True(response.IsSuccessStatusCode);
```

---

## 🐳 Generic Docker Containers — `NotoriousTest.TestContainers`

Provides a base class to integrate any [Testcontainers](https://dotnet.testcontainers.org/) container that is not a database.

### Installation

```
dotnet add package NotoriousTest.TestContainers
```

### Usage

Inherit from `DockerContainerInfrastructure<TContainer, TOutputConfiguration>`, where `TContainer` is any `IContainer` from the Testcontainers library:

```csharp
public class RedisInfrastructure : DockerContainerInfrastructure<RedisContainer, RedisConfig>
{
    public RedisInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        Container = new RedisBuilder().Build();
    }

    public override async Task Initialize()
    {
        await base.Initialize(); // starts the container

        AddEntry("Redis:ConnectionString", new RedisConfig
        {
            ConnectionString = Container.GetConnectionString()
        });
    }

    public override Task Reset() => Task.CompletedTask;
}
```

📌 **Key Points:**
- `base.Initialize()` starts the container and registers its metadata (container ID, name) for DoggyDog crash recovery.
- `base.Destroy()` force-removes the container via the Docker client.
- The `[Cleaner]` attribute is already set on the base class — Docker containers are cleaned up automatically after a crash.
- `TESTCONTAINERS_RYUK_DISABLED` is set to `true` by default to prevent conflicts with Ryuk.

---

## 🗄️ SQL Server — `NotoriousTest.SqlServer`

Provides two variants: a **Docker** variant (Testcontainers) and an **external** variant (existing SQL Server instance).

Both variants:
- Create a unique database per test session (named `{DbPrefix}_{EnvironmentId}`)
- Reset the database between each test using [Respawn](https://github.com/jbogard/Respawn)
- Drop the database at the end of the session
- Expose the connection string as a `ConfigurationEntry` for automatic injection into the web app

### Installation

```
dotnet add package NotoriousTest.SqlServer
```

---

### Docker variant

`SqlServerContainerInfrastructure` spins up a SQL Server Docker container using Testcontainers.

**Minimal setup** — just inherit, nothing else required:

```csharp
public class MyDatabaseInfrastructure : SqlServerContainerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }
}
```

**Override `Initialize()`** to create your schema after the database is ready:

```csharp
public class MyDatabaseInfrastructure : SqlServerContainerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize(); // starts container, creates database, registers connection string

        using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE Users (Id INT PRIMARY KEY, Name NVARCHAR(100))";
        await command.ExecuteNonQueryAsync();
    }
}
```

**Customization:**

```csharp
public class MyDatabaseInfrastructure : SqlServerContainerInfrastructure
{
    // Override the appsettings key used to expose the connection string
    protected override string ConnectionStringKey => "ConnectionStrings:MyDb";

    // Override the database name prefix (default: "NotoriousDb")
    public override string DbPrefix => "MyApp";

    // Customize the Docker container
    protected override MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        => builder.WithPassword("MyStr0ng!Password");

    // Exclude migration tables from Respawn reset
    public override string[] TableToIgnore => ["__EFMigrationsHistory"];

    // Filter schemas for Respawn
    public string[] SchemasToInclude => ["dbo"];
}
```

---

### External variant

`SqlServerInfrastructure` connects to an existing SQL Server instance. The connection string is loaded from `testsettings.json` via `SettingsExtension`.

**`testsettings.json`** (must be copied to output directory):

```json
{
  "SqlServerInfrastructure": {
    "ConnectionString": "Server=localhost;User Id=sa;Password=...;TrustServerCertificate=True"
  }
}
```

**Usage:**

```csharp
public class MyDatabaseInfrastructure : SqlServerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestSettingsProvider settings,
                                    ITestLogger logger, IRegistry registry)
        : base(contextId, settings, logger, registry) { }
}
```

> The settings section key defaults to the class name. Override `SectionName` in the inherited `SettingsExtension` to use a custom key.

---

### Using in Tests

Both variants expose the same methods:

```csharp
var infra = CurrentEnvironment.GetInfrastructure<MyDatabaseInfrastructure>();

// Get a DbConnection pointing to the test database
await using var connection = infra.GetDatabaseConnection();
await connection.OpenAsync();

// Or get the connection string directly
string connectionString = infra.GetDatabaseConnectionString();
```

---

## 🐘 PostgreSQL — `NotoriousTest.PostgreSql`

Same structure as SQL Server, with a **Docker** variant and an **external** variant.

### Installation

```
dotnet add package NotoriousTest.PostgreSql
```

---

### Docker variant

`PostgreContainerInfrastructure` spins up a PostgreSQL Docker container using Testcontainers.

```csharp
public class MyDatabaseInfrastructure : PostgreContainerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize(); // starts container, creates database, registers connection string

        using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE users (id SERIAL PRIMARY KEY, name TEXT)";
        await command.ExecuteNonQueryAsync();
    }
}
```

**Customization:**

```csharp
public class MyDatabaseInfrastructure : PostgreContainerInfrastructure
{
    protected override string ConnectionStringKey => "ConnectionStrings:MyDb";
    public override string DbPrefix => "MyApp";

    protected override PostgreSqlBuilder ConfigureSqlContainer(PostgreSqlBuilder builder)
        => builder.WithPassword("mysecretpassword");

    public string[] SchemasToInclude => ["public"];
}
```

---

### External variant

`PostgreInfrastructure` connects to an existing PostgreSQL server. The connection string is loaded from `testsettings.json`.

**`testsettings.json`:**

```json
{
  "PostgreInfrastructure": {
    "ConnectionString": "Host=localhost;Port=5432;Username=postgres;Password=..."
  }
}
```

**Usage:**

```csharp
public class MyDatabaseInfrastructure : PostgreInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestSettingsProvider settings,
                                    ITestLogger logger, IRegistry registry)
        : base(contextId, settings, logger, registry) { }
}
```

---

### Using in Tests

```csharp
var infra = CurrentEnvironment.GetInfrastructure<MyDatabaseInfrastructure>();

await using var connection = (NpgsqlConnection)infra.GetDatabaseConnection();
await connection.OpenAsync();
```

---

## 🪶 SQLite — `NotoriousTest.Sqlite`

`SqliteInfrastructure` creates a unique SQLite database file per test session, and deletes it at the end.

> SQLite has no Docker variant — it is file-based by nature.

### Installation

```
dotnet add package NotoriousTest.Sqlite
```

### Configuration

**`testsettings.json`:**

```json
{
  "SqliteInfrastructure": {
    "ConnectionString": "Data Source=tests.db"
  }
}
```

The connection string `Data Source` is used as the base path. The infrastructure appends `_{EnvironmentId}` to the filename to ensure uniqueness per session (e.g. `tests_abc123.db`).

### Usage

```csharp
public class MyDatabaseInfrastructure : SqliteInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestSettingsProvider settings,
                                    ITestLogger logger, IRegistry registry)
        : base(contextId, settings, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize(); // creates the database file

        using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE users (id INTEGER PRIMARY KEY, name TEXT)";
        await command.ExecuteNonQueryAsync();
    }
}
```

### Using in Tests

```csharp
var infra = CurrentEnvironment.GetInfrastructure<MyDatabaseInfrastructure>();

await using var connection = infra.GetDatabaseConnection();
await connection.OpenAsync();
```

📌 **Key Points:**
- The database file is automatically deleted on `Destroy()`.
- Reset between tests is handled by Respawn (table-level reset, not file deletion).
- `GetDatabaseConnectionString()` and `GetServerConnectionString()` return the same value — SQLite makes no distinction.

---

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
