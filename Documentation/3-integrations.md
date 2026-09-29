# 🔌 Integrations

NotoriousTest ships ready-to-use infrastructures for the most common external dependencies. Each one is a regular
[infrastructure](./2-core-concepts.md#1-infrastructure-environments-and-tests): inherit from it, add it to your
environment, and NotoriousTest handles its lifecycle.

## Summary

1. [Available Packages](#1-available-packages)
2. [Web Applications](#2-web-applications)
   1. [ASP.NET Core](#21-aspnet-core)
   2. [Azure Functions](#22-azure-functions)
3. [Docker Containers](#3-docker-containers)
4. [Databases](#4-databases)
   1. [Common Behavior](#41-common-behavior)
   2. [SQL Server](#42-sql-server)
   3. [PostgreSQL](#43-postgresql)
   4. [SQLite](#44-sqlite)
5. [Requirements & Dependencies](#5-requirements--dependencies)

---

## 1. Available Packages

| Package | Provides |
|---------|----------|
| `NotoriousTest.Web` | `WebApplication<TEntryPoint>` — an in-memory ASP.NET Core server. |
| `NotoriousTest.Web.AzureFunctions` | `AzureFunctionWebApplication` — a local Azure Functions host (`func host start`). |
| `NotoriousTest.TestContainers` | `DockerContainerInfrastructure<TContainer, TOutputConfiguration>` — any [Testcontainers](https://dotnet.testcontainers.org/) container. |
| `NotoriousTest.Database` | Base classes shared by all database integrations (Docker & external). |
| `NotoriousTest.SqlServer` | SQL Server — in a Docker container or on an existing server. |
| `NotoriousTest.PostgreSql` | PostgreSQL — in a Docker container or on an existing server. |
| `NotoriousTest.Sqlite` | SQLite — one database file per environment. |
| `NotoriousTest.Requirements.Docker` | `DockerRequirement` — checks Docker is installed and running. |
| `NotoriousTest.Dependencies.Azure.FunctionCoreTools` | `AzureFunctionCoreToolsDependency` — installs the `func` CLI. |

Each package is installed alongside your test framework package (`NotoriousTest.XUnit`, `NotoriousTest.NUnit`,
`NotoriousTest.MSTest` or `NotoriousTest.TUnit`):

```sh
dotnet add package NotoriousTest.SqlServer
```

---

## 2. Web Applications

A web application is the system under test: your tests call it over HTTP, and it talks to the other infrastructures
(databases, containers…). NotoriousTest wraps it in a **web application infrastructure** that:

- starts **last** (`Order = 999`), once every other infrastructure is running;
- is a [configuration consumer](./2-core-concepts.md#14-configuration): every configuration entry produced by the other
  infrastructures (connection strings, URLs…) is injected into the application's configuration;
- exposes an `HttpClient` pointing to the running application;
- is **not** tracked by DoggyDog (`DisableRegistry = true`): the application runs in memory or as a child process and
  dies with the test process.

Register a web application with `AddWebApplication<T>()` and retrieve it with `GetWebApplication()`:

```csharp
using NotoriousTest.Web;

public override Task ConfigureEnvironment()
{
    AddInfrastructure<MyDatabaseInfrastructure>(); // produces "ConnectionStrings:Default"
    this.AddWebApplication<MyWebApplication>();     // receives it in its IConfiguration
    return Task.CompletedTask;
}
```

```csharp
[Fact]
public async Task GetUsers_Should_ReturnOk()
{
    HttpClient client = Environment.GetWebApplication().HttpClient!;

    HttpResponseMessage response = await client.GetAsync("users");

    Assert.True(response.IsSuccessStatusCode);
}
```

> 💡 `GetWebApplication()` returns the first web application of the environment. If you register several, retrieve a
> specific one with `Environment.GetInfrastructure<WebApplicationInfrastructure<MyOtherWebApplication>>()`.

---

### 2.1 ASP.NET Core

`NotoriousTest.Web`

Create a class inheriting from `WebApplication<TEntryPoint>`, where `TEntryPoint` is the `Program` class of your API.
It is a `WebApplicationFactory<TEntryPoint>`: the application runs in memory, in the test process.

```csharp
using NotoriousTest.Web.Applications;

public class MyWebApplication : WebApplication<Program>
{
}
```

That's all: the configuration produced by the other infrastructures is added to the application's `IConfiguration`
automatically.

**How the configuration is injected:** each entry is flattened into `appsettings`-style keys, then added as an
in-memory configuration source.

| Entry key | Entry value | Resulting configuration |
|-----------|-------------|-------------------------|
| `ConnectionStrings:Default` | `"Server=..."` | `ConnectionStrings:Default = Server=...` |
| `Redis` | `new { Host = "localhost", Port = 6379 }` | `Redis:Host = localhost`, `Redis:Port = 6379` |
| `Brokers` | `new[] { "a", "b" }` | `Brokers:0 = a`, `Brokers:1 = b` |

**Customizing the application** — override any `WebApplicationFactory` method. Keep the call to
`base.ConfigureWebHost()`, it is what injects the configuration:

```csharp
public class MyWebApplication : WebApplication<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder); // injects the consumed configuration

        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEmailSender, FakeEmailSender>();
        });
    }
}
```

**Customizing the `HttpClient`** — override `Start()`:

```csharp
public class MyWebApplication : WebApplication<Program>
{
    public override Task<HttpClient> Start() =>
        Task.FromResult(CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        }));
}
```

> ⚠️ Your API project must expose its `Program` class to the test project, e.g. with `public partial class Program { }`
> at the end of `Program.cs`.

---

### 2.2 Azure Functions

`NotoriousTest.Web.AzureFunctions`

`AzureFunctionWebApplication` starts your Azure Functions project (isolated worker) with the Azure Functions Core Tools
(`func host start`) and gives you an `HttpClient` pointing to it.

```csharp
using NotoriousTest.Web.AzureFunctions;

public class MyFunctionApplication : AzureFunctionWebApplication
{
    // Path to the Azure Functions project, from the test output directory (bin/Debug/net10.0)
    public override string FunctionProjectDir { get; } = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "../../../../MyApp.Functions"));
}
```

```csharp
public override Task ConfigureEnvironment()
{
    AddInfrastructure<MyDatabaseInfrastructure>();
    this.AddWebApplication<MyFunctionApplication>();
    return Task.CompletedTask;
}
```

📌 **How it works:**

1. The `func` CLI is declared as a [dependency](./2-core-concepts.md#13-dependencies): if it is missing, it is installed
   before the campaign (and uninstalled at the end).
2. A free local port is picked, and `func host start --port <port>` is run in `FunctionProjectDir` (which also builds
   the project).
3. The configuration produced by the other infrastructures is passed as **environment variables**, flattened with `__`
   (`ConnectionStrings__Default`), which is what Azure Functions expects.
4. NotoriousTest polls `/admin/host/status` until the host is `Running`. If the process exits or the timeout is reached,
   initialization fails with the host logs in the exception message.
5. On `Destroy()`, the host process and its worker are killed.

**Customization:**

| Member | Default | Description |
|--------|---------|-------------|
| `FunctionProjectDir` | — (required) | Directory of the Azure Functions project. |
| `AdditionalConfiguration` | empty | Extra environment variables given to the host (e.g. `AzureWebJobsStorage`). |
| `StartupTimeout` | 2 minutes | Maximum time to wait for the host to be running, build included. |
| `Logs` | — | Standard and error output of the host (protected). |

```csharp
public class MyFunctionApplication : AzureFunctionWebApplication
{
    public override string FunctionProjectDir { get; } = "...";

    public override Dictionary<string, string> AdditionalConfiguration { get; set; } = new()
    {
        ["AzureWebJobsStorage"] = "UseDevelopmentStorage=true"
    };

    protected override TimeSpan StartupTimeout => TimeSpan.FromMinutes(5);
}
```

> ⚠️ Installing the `func` CLI requires elevated privileges (UAC prompt with `winget` on Windows, root or passwordless
> `sudo` with `apt-get` on Debian/Ubuntu). On a CI agent, prefer installing it beforehand: an existing installation is
> detected and left untouched.

---

## 3. Docker Containers

`NotoriousTest.TestContainers`

`DockerContainerInfrastructure<TContainer, TOutputConfiguration>` runs any [Testcontainers](https://dotnet.testcontainers.org/)
container: Redis, RabbitMQ, Azurite, Keycloak, a mock server…

Build the container in the constructor, and publish what the other infrastructures need with `AddEntry()`:

```csharp
using NotoriousTest.Requirements.Docker;
using NotoriousTest.TestContainers;
using Testcontainers.Redis;

public class RedisInfrastructure : DockerContainerInfrastructure<RedisContainer, string>
{
    public RedisInfrastructure(EnvironmentId environmentId, ITestLogger logger, IRegistry registry)
        : base(environmentId, logger, registry)
    {
        Container = new RedisBuilder().Build();
        Requirements.Add(new DockerRequirement()); // fail fast if Docker is not running
    }

    public override async Task Initialize()
    {
        await base.Initialize(); // starts the container

        AddEntry("ConnectionStrings:Redis", Container.GetConnectionString());
    }

    public override async Task Reset()
    {
        // Restore a clean state between tests, e.g. flush the cache
        await Container.ExecAsync(["redis-cli", "FLUSHALL"]);
    }
}
```

📌 **Key Points:**

- `base.Initialize()` starts the container and registers it in the DoggyDog registry with its id and name.
- `base.Destroy()` force-removes the container.
- `Reset()` does nothing by default: implement it if the container holds state.
- A cleaner is already provided: after a crash, DoggyDog removes the container.
- `TOutputConfiguration` is the type of the entries you publish — `string` for simple values, or your own class.
- Ryuk (the Testcontainers resource reaper) is disabled (`TESTCONTAINERS_RYUK_DISABLED=true`): container cleanup is
  handled by NotoriousTest and DoggyDog.

---

## 4. Databases

Every database integration comes in two flavors:

| Flavor | Class | When to use |
|--------|-------|-------------|
| **Docker** | `SqlServerContainerInfrastructure`, `PostgreContainerInfrastructure` | Nothing to install except Docker. Best for local runs and most CI. |
| **External** | `SqlServerInfrastructure`, `PostgreInfrastructure`, `SqliteInfrastructure` | You already have a server (or a file, for SQLite). The connection string comes from `testsettings.json`. |

---

### 4.1 Common Behavior

All database infrastructures share the same lifecycle, inherited from `NotoriousTest.Database`:

| Step | Docker | External |
|------|--------|----------|
| **Initialize** | Starts the container, then creates the database. | Creates the database on the server. |
| **Reset** | Empties every table with [Respawn](https://github.com/jbogard/Respawn). | Same. |
| **Destroy** | Removes the container (and the database with it). | Drops the database. |
| **Crash recovery** | DoggyDog removes the container. | DoggyDog drops the database. |
| **Requirements** | [`DockerRequirement`](#5-requirements--dependencies), declared automatically. | None. |

**One database per environment:** the database is named `{DbPrefix}_{EnvironmentId}` (e.g.
`NotoriousDb_3f2b9c1e-...`). Parallel test classes or campaigns never share data.

**Connection string published automatically:** the database connection string is published as a
[configuration entry](./2-core-concepts.md#14-configuration) under `ConnectionStrings:Default`, so a
[web application](#2-web-applications) receives it with no code. Override `ConnectionStringKey` to change the key.

**Creating your schema:** override `Initialize()`, call `base.Initialize()`, then run your scripts or migrations:

```csharp
public class MyDatabaseInfrastructure : SqlServerContainerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestLogger logger, IRegistry registry)
        : base(environmentId, logger, registry) { }

    protected override string ConnectionStringKey => "ConnectionStrings:MyDb";

    public override async Task Initialize()
    {
        await base.Initialize(); // database is created and ready

        await using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE Users (Id INT PRIMARY KEY, Name NVARCHAR(100))";
        await command.ExecuteNonQueryAsync();
    }
}
```

**Tuning the reset and the database name** — these properties are `init`-only: set them in your constructor.

| Property | Default | Description |
|----------|---------|-------------|
| `DbPrefix` | `"NotoriousDb"` | Prefix of the database name. |
| `TableToIgnore` | `[]` | Tables never emptied (e.g. `__EFMigrationsHistory`, reference data). |
| `TableToInclude` | `[]` | If set, only these tables are emptied. |
| `SchemasToInclude` | `[]` | If set, only tables of these schemas are emptied. |
| `SchemasToExclude` | `[]` | Tables of these schemas are never emptied. |

```csharp
public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestLogger logger, IRegistry registry)
    : base(environmentId, logger, registry)
{
    DbPrefix = "MyApp";
    TableToIgnore = ["__EFMigrationsHistory", "Countries"];
}
```

**Using the database in tests:**

```csharp
var database = Environment.GetInfrastructure<MyDatabaseInfrastructure>();

await using DbConnection connection = database.GetDatabaseConnection();
await connection.OpenAsync();
```

| Method | Returns |
|--------|---------|
| `GetDatabaseConnection()` | A `DbConnection` (not opened) to the test database. |
| `GetDatabaseConnectionString()` | The connection string of the test database. |
| `GetServerConnection()` | A `DbConnection` to the server, without database. |
| `GetServerConnectionString()` | The connection string of the server. |

**Publishing your own configuration type:** the non-generic classes publish a `string`. To publish something else, use
the generic variant (`SqlServerContainerInfrastructure<TOutputConfiguration>`,
`SqlServerInfrastructure<TOutputConfiguration, TSettings>`…) and call `AddEntry()` yourself:

```csharp
public class MyDbConfig
{
    public string ConnectionString { get; set; } = "";
    public int CommandTimeout { get; set; }
}

public class MyDatabaseInfrastructure : SqlServerContainerInfrastructure<MyDbConfig>
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestLogger logger, IRegistry registry)
        : base(environmentId, logger, registry) { }

    public override async Task Initialize()
    {
        await base.Initialize();

        // Flattened into "Database:ConnectionString" and "Database:CommandTimeout"
        AddEntry("Database", new MyDbConfig
        {
            ConnectionString = GetDatabaseConnectionString(),
            CommandTimeout = 30
        });
    }
}
```

---

### 4.2 SQL Server

`NotoriousTest.SqlServer`

#### Docker — `SqlServerContainerInfrastructure`

Starts a SQL Server container ([Testcontainers.MsSql](https://dotnet.testcontainers.org/modules/mssql/)).

```csharp
public class MyDatabaseInfrastructure : SqlServerContainerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestLogger logger, IRegistry registry)
        : base(environmentId, logger, registry) { }
}
```

Customize the container by overriding `ConfigureSqlContainer()`:

```csharp
protected override MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder) =>
    builder
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("MyStr0ng!Password");
```

#### External — `SqlServerInfrastructure`

Creates the test database on an existing SQL Server. The connection string is read from
[`testsettings.json`](./2-core-concepts.md#3-settings), in a section named after **your infrastructure class**:

```json
{
  "MyDatabaseInfrastructure": {
    "ConnectionString": "Server=localhost;User Id=sa;Password=...;TrustServerCertificate=True"
  }
}
```

```csharp
public class MyDatabaseInfrastructure : SqlServerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestSettingsProvider settingsProvider,
        ITestLogger logger, IRegistry registry)
        : base(environmentId, settingsProvider, logger, registry) { }
}
```

> ⚠️ The login must be allowed to create and drop databases. If the section is missing, an
> `InfrastructureSettingsNotFound` exception is thrown.

---

### 4.3 PostgreSQL

`NotoriousTest.PostgreSql`

#### Docker — `PostgreContainerInfrastructure`

Starts a PostgreSQL container ([Testcontainers.PostgreSql](https://dotnet.testcontainers.org/modules/postgres/)).

```csharp
public class MyDatabaseInfrastructure : PostgreContainerInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestLogger logger, IRegistry registry)
        : base(environmentId, logger, registry) { }

    protected override PostgreSqlBuilder ConfigureSqlContainer(PostgreSqlBuilder builder) =>
        builder.WithImage("postgres:17");
}
```

#### External — `PostgreInfrastructure`

Creates the test database on an existing PostgreSQL server:

```json
{
  "MyDatabaseInfrastructure": {
    "ConnectionString": "Host=localhost;Port=5432;Username=postgres;Password=..."
  }
}
```

```csharp
public class MyDatabaseInfrastructure : PostgreInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestSettingsProvider settingsProvider,
        ITestLogger logger, IRegistry registry)
        : base(environmentId, settingsProvider, logger, registry) { }
}
```

Connections are `NpgsqlConnection` instances: cast `GetDatabaseConnection()` if you need Npgsql-specific features.

---

### 4.4 SQLite

`NotoriousTest.Sqlite`

`SqliteInfrastructure` creates one SQLite **file** per environment, and deletes it at the end. SQLite is file-based:
there is no Docker flavor.

```json
{
  "MyDatabaseInfrastructure": {
    "ConnectionString": "Data Source=tests.db"
  }
}
```

```csharp
public class MyDatabaseInfrastructure : SqliteInfrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId environmentId, ITestSettingsProvider settingsProvider,
        ITestLogger logger, IRegistry registry)
        : base(environmentId, settingsProvider, logger, registry) { }
}
```

📌 **Key Points:**

- The `Data Source` of the settings is used as a template: the database name is appended to the file name, e.g.
  `tests_NotoriousDb_3f2b9c1e-....db`. `GetPath()` returns the actual file path.
- The file is created on first connection, emptied with Respawn on `Reset()`, and deleted on `Destroy()` — or by
  DoggyDog after a crash.
- `GetServerConnectionString()` and `GetDatabaseConnectionString()` return the same value.

---

## 5. Requirements & Dependencies

These packages provide ready-made [requirements](./2-core-concepts.md#12-requirements) and
[dependencies](./2-core-concepts.md#13-dependencies) to declare on your infrastructures.

### `NotoriousTest.Requirements.Docker`

`DockerRequirement` checks that the Docker CLI is installed **and** that the daemon is running (`docker info`). The
Docker database infrastructures (`SqlServerContainerInfrastructure`, `PostgreContainerInfrastructure`) already declare
it. Add it to your own Docker-based infrastructures to get a clear error before anything starts, instead of a
Testcontainers timeout:

```csharp
Requirements.Add(new DockerRequirement());
```

Docker is never installed automatically: it needs elevated privileges, sometimes a reboot, and a license for Docker
Desktop.

### `NotoriousTest.Dependencies.Azure.FunctionCoreTools`

`AzureFunctionCoreToolsDependency` installs the Azure Functions Core Tools (`func` CLI) if missing, and uninstalls it at
the end of the campaign if NotoriousTest installed it.

| OS | Installation |
|----|--------------|
| Windows | `winget install Microsoft.Azure.FunctionsCoreTools` (UAC prompt). |
| Debian / Ubuntu | `apt-get install azure-functions-core-tools-4` from the Microsoft repository (root or passwordless `sudo`). |
| Other | Not supported: install it manually. |

It is already declared by [`AzureFunctionWebApplication`](#22-azure-functions).

---

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions)
or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
