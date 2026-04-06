# 📚 Example — Web API + SQL Server

This example walks through a complete, real-world integration test setup: a web API that creates users in a SQL Server database.

The full working code is available in the [`Samples/NotoriousTest.Sample.XUnit`](../Samples/NotoriousTest.Sample.XUnit/) folder.

---

## The Application Under Test

The application exposes a single endpoint that inserts a user into a SQL Server database, reading the connection string from `IConfiguration`:

```csharp
// Controllers/UserController.cs
[ApiController]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public void CreateUser()
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("SqlServer"));
        connection.Open();

        using var command = connection.CreateCommand();
        command.Parameters.AddWithValue("@username", $"user_{Random.Shared.Next()}");
        command.Parameters.AddWithValue("@email", $"user_{Random.Shared.Next()}@example.com");
        command.Parameters.AddWithValue("@password_hash", "hash");
        command.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
        command.CommandText = @"INSERT INTO Users (username, email, password_hash, created_at)
                                VALUES (@username, @email, @password_hash, @created_at)";
        command.ExecuteNonQuery();
    }
}
```

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var app = builder.Build();
app.MapControllers();
app.Run();

public partial class Program { }
```

---

## Setup

Create an xUnit test project and install the required packages:

```
dotnet add package NotoriousTest.XUnit
dotnet add package NotoriousTest.SqlServer
dotnet add package NotoriousTest.Web
```

---

## Step 1 — Define the Infrastructure

Inherit from `SqlServerContainerInfrastructure`. Override `Initialize()` to create the schema after the database is ready.

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    // Key used to expose the connection string to the web application
    protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";

    public override async Task Initialize()
    {
        await base.Initialize(); // starts container, creates database, outputs connection string

        using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE Users (
                user_id       INT IDENTITY(1,1) PRIMARY KEY,
                username      NVARCHAR(50)  NOT NULL UNIQUE,
                email         NVARCHAR(100) NOT NULL UNIQUE,
                password_hash NVARCHAR(255) NOT NULL,
                created_at    DATETIME DEFAULT GETDATE()
            )";
        await command.ExecuteNonQueryAsync();
    }
}
```

What `base.Initialize()` does automatically:
- Starts a SQL Server Docker container via Testcontainers.
- Creates a unique database named `NotoriousDb_{EnvironmentId}`.
- Registers the database in DoggyDog for crash recovery.
- Publishes the connection string as a `ConfigurationEntry` under `ConnectionStrings:SqlServer`.

What happens on `Reset()` (before each test):
- Respawn empties all tables, leaving the schema intact.

What happens on `Destroy()` (end of session):
- The Docker container is stopped and removed.

---

## Step 2 — Define the Web Application

```csharp
public class TestWebApplication : WebApplication<Program>
{
    // Override WebApplicationFactory methods here if needed
}
```

`WebApplication<Program>` automatically receives all `ConfigurationEntry` objects produced by other infrastructures and injects them into the app's `IConfiguration`. The `ConnectionStrings:SqlServer` entry published by `SqlServerInfrastructure` will be available to the controller without any extra wiring.

---

## Step 3 — Create the Environment

```csharp
public class TestEnvironment : NotoriousTest.XUnit.Environment
{
    public TestEnvironment(IMessageSink sink) : base(sink) { }

    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<SqlServerInfrastructure>();
        this.AddWebApplication<TestWebApplication>();
    }
}
```

The environment:
1. Resolves `SqlServerInfrastructure` via the internal DI container (all constructor parameters injected automatically).
2. Initializes infrastructures in order — `SqlServerInfrastructure` first (produces config), `WebApplicationInfrastructure` last (consumes it).
3. Launches DoggyDog to watch the process and clean up containers on crash.

---

## Step 4 — Write the Tests

```csharp
public class UserTests : NotoriousTest.XUnit.IntegrationTest<TestEnvironment>
{
    public UserTests(TestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task CreateUser_ShouldInsertOneRow()
    {
        // Act — call the API
        HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
        HttpResponseMessage response = await client.PostAsync("users", null);

        // Assert — HTTP response
        Assert.True(response.IsSuccessStatusCode);

        // Assert — database state
        SqlServerInfrastructure db = CurrentEnvironment.GetInfrastructure<SqlServerInfrastructure>();
        await using var connection = db.GetDatabaseConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users";
        int count = (int)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateUser_CalledTwice_ShouldStillHaveOneRow()
    {
        // Each test starts with a clean database — Respawn ran before this test
        HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
        await client.PostAsync("users", null);

        SqlServerInfrastructure db = CurrentEnvironment.GetInfrastructure<SqlServerInfrastructure>();
        await using var connection = db.GetDatabaseConnection();
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users";
        int count = (int)await command.ExecuteScalarAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, count);
    }
}
```

Each test runs against a fresh database — Respawn resets the data between tests without recreating the schema.

---

## Running the Tests

```sh
dotnet test
```

NotoriousTest handles the full lifecycle:

```
Session starts
  └─ SQL Server container starts (Testcontainers)
  └─ Database "NotoriousDb_{id}" is created
  └─ Users table is created
  └─ Web application starts, connection string injected

Before each test
  └─ Respawn empties all tables

Each test runs in isolation

Session ends
  └─ Container is stopped and removed
```

---

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
