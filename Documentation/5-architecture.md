# 🏛️ Architecture Guidelines

NotoriousTest gives you three complementary tools: **Infrastructures**, **Extensions**, and **Dependency Injection**.
Each has a distinct responsibility. Keeping them separated makes your test setup readable, reusable, and easy to maintain.

## Summary

- [The Three Layers](#the-three-layers)
  - [Infrastructure — Server Setup](#infrastructure--server-setup)
  - [Extensions — Application-Specific Behavior](#extensions--application-specific-behavior)
  - [Dependency Injection — Decoupling Implementations](#dependency-injection--decoupling-implementations)
- [The Full Picture](#the-full-picture)
- [Integration Test Frameworks](#integration-test-frameworks)
  - [Arrange — Seeding Test Data](#arrange--seeding-test-data)
  - [Act — Calling the Application](#act--calling-the-application)
  - [Assert — Verifying State](#assert--verifying-state)
  - [Creating a Base Test Class](#creating-a-base-test-class)
  - [Writing Tests](#writing-tests)

---

## The Three Layers

### Infrastructure — Server Setup

An infrastructure's job is to **bring a server or service online**.
It knows how to start it, reach it, and shut it down. Nothing more.

✅ What belongs here:
- Starting and stopping a Docker container
- Creating and dropping a database
- Registering metadata for DoggyDog crash recovery

❌ What does not belong here:
- Creating tables or running migrations
- Seeding data
- Any logic specific to your application

```csharp
// ✅ An infrastructure that owns the server lifecycle
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    protected override string ConnectionStringKey => "ConnectionStrings:MyApp";

    public override async Task Initialize()
    {
        await base.Initialize(); // start container, create database, publish connection string
        // nothing else — schema is the app's concern, not the server's
    }
}
```

---

### Extensions — Application-Specific Behavior

An extension's job is to **add behavior that is specific to your application** on top of an infrastructure.
It reacts to infrastructure lifecycle events (after initialization, before reset, etc.) and acts on them with application knowledge.

✅ What belongs here:
- Running schema migrations after the database is ready
- Seeding reference or test data
- Any setup that depends on the application's domain

```csharp
// ✅ An extension that owns the schema
public class SchemaExtension : IInfrastructureExtension<SqlServerInfrastructure>
{
    private readonly IDbMigrator _migrator;

    public SchemaExtension(IDbMigrator migrator)
    {
        _migrator = migrator;
    }

    public async Task OnAfterInitialize(SqlServerInfrastructure infrastructure)
    {
        string connectionString = infrastructure.GetDatabaseConnectionString();
        await _migrator.MigrateAsync(connectionString);
    }
}

// ✅ An extension that owns the reference data
public class ReferenceDataExtension : IInfrastructureExtension<SqlServerInfrastructure>
{
    private readonly IDataSeeder _seeder;

    public ReferenceDataExtension(IDataSeeder seeder)
    {
        _seeder = seeder;
    }

    public async Task OnAfterInitialize(SqlServerInfrastructure infrastructure)
    {
        using var connection = infrastructure.GetDatabaseConnection();
        await connection.OpenAsync();
        await _seeder.SeedAsync(connection);
    }
}
```

Register them in the infrastructure's constructor:

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry,
                                   IDbMigrator migrator, IDataSeeder seeder)
        : base(contextId, logger, registry)
    {
        EnsureExtension(new SchemaExtension(migrator));
        EnsureExtension(new ReferenceDataExtension(seeder));
    }
}
```

---

### Dependency Injection — Decoupling Implementations

DI's job is to **provide implementations without coupling your infrastructures or extensions to concrete classes**.
Register your services once in the environment, and they flow automatically into any infrastructure or extension constructor.

✅ What belongs here:
- Migration tools (`DbUp`, `FluentMigrator`, `EF Core`, etc.)
- Custom seeders
- Any shared utility that multiple infrastructures or extensions consume

```csharp
public class MyTestEnvironment : NotoriousTest.XUnit.Environment
{
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override void ConfigureInfrastructureServices(IServiceCollection services)
    {
        base.ConfigureInfrastructureServices(services);

        services.AddSingleton<IDbMigrator, DbUpMigrator>();
        services.AddSingleton<IDataSeeder, ReferenceDataSeeder>();
    }

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<SqlServerInfrastructure>(); // receives IDbMigrator and IDataSeeder via DI
        this.AddWebApplication<TestWebApplication>();
    }
}
```

Because `IDbMigrator` and `IDataSeeder` are registered as interfaces, you can swap implementations without touching the infrastructure or extensions — useful when sharing a setup across multiple test projects with different migration strategies.

---

## The Full Picture

```
Environment
  └─ DI container
      ├─ IDbMigrator       → DbUpMigrator
      └─ IDataSeeder       → ReferenceDataSeeder

  └─ SqlServerInfrastructure      ← server concern
      ├─ SchemaExtension          ← app concern (uses IDbMigrator)
      └─ ReferenceDataExtension   ← app concern (uses IDataSeeder)

  └─ WebApplicationInfrastructure
      └─ receives ConnectionStrings:MyApp automatically
```

Each layer has one reason to change:
- The **infrastructure** changes when the server changes (different image, different port, etc.).
- An **extension** changes when the application schema or data changes.
- The **DI registrations** change when you swap a tool or share the setup across projects.

---

## Integration Test Frameworks

As your test suite grows, you'll find yourself repeating the same infrastructure retrieval and action logic across many tests.
The recommended pattern is to build **test framework classes** — one per phase of the AAA pattern (Arrange, Act, Assert) — that hold a reference to the environment and expose meaningful, domain-oriented methods.

Tests never touch infrastructures directly. They delegate to the framework.

---

### Arrange — Seeding Test Data

An `Arrange` framework prepares the database state before a test. It writes directly to the infrastructure, bypassing the API.

```csharp
public class UserArrange
{
    private readonly TestEnvironment _environment;

    public UserArrange(TestEnvironment environment)
    {
        _environment = environment;
    }

    public async Task WithExistingUser(string username, string email)
    {
        var db = _environment.GetInfrastructure<SqlServerInfrastructure>();
        await using var connection = db.GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"INSERT INTO Users (username, email, password_hash, created_at)
                                VALUES (@username, @email, 'hash', GETDATE())";
        command.Parameters.AddWithValue("@username", username);
        command.Parameters.AddWithValue("@email", email);
        await command.ExecuteNonQueryAsync();
    }
}
```

---

### Act — Calling the Application

An `Act` framework wraps HTTP calls with meaningful, typed methods. It hides URL construction, serialization, and `HttpClient` access.

```csharp
public class UserAct
{
    private readonly TestEnvironment _environment;

    public UserAct(TestEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<HttpResponseMessage> CreateUser(string username, string email)
    {
        var client = _environment.GetWebApplication().HttpClient;
        return await client.PostAsync($"users?username={username}&email={email}", null);
    }

    public async Task<HttpResponseMessage> DeleteUser(int userId)
    {
        var client = _environment.GetWebApplication().HttpClient;
        return await client.DeleteAsync($"users/{userId}");
    }
}
```

---

### Assert — Verifying State

An `Assert` framework checks the expected state of the system after an action. It reads from the infrastructure and expresses expectations in domain terms.

```csharp
public class UserAssert
{
    private readonly TestEnvironment _environment;

    public UserAssert(TestEnvironment environment)
    {
        _environment = environment;
    }

    public async Task CountIs(int expected)
    {
        var db = _environment.GetInfrastructure<SqlServerInfrastructure>();
        await using var connection = db.GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users";
        int actual = (int)await command.ExecuteScalarAsync();

        Assert.Equal(expected, actual);
    }

    public async Task Exists(string username)
    {
        var db = _environment.GetInfrastructure<SqlServerInfrastructure>();
        await using var connection = db.GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Users WHERE username = @username";
        command.Parameters.AddWithValue("@username", username);
        int count = (int)await command.ExecuteScalarAsync();

        Assert.Equal(1, count);
    }
}
```

---

### Creating a Base Test Class

Instantiate all three frameworks in a shared base test class. This is the only place that knows about the environment.

```csharp
public abstract class MyAppIntegrationTest : NotoriousTest.XUnit.IntegrationTest<TestEnvironment>
{
    protected UserArrange Arrange { get; }
    protected UserAct Act { get; }
    protected UserAssert Assert { get; }

    protected MyAppIntegrationTest(TestEnvironment environment) : base(environment)
    {
        Arrange = new UserArrange(environment);
        Act     = new UserAct(environment);
        Assert  = new UserAssert(environment);
    }
}
```

---

### Writing Tests

Tests inherit from the base class and read as plain specifications.

```csharp
public class UserTests : MyAppIntegrationTest
{
    public UserTests(TestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task CreateUser_ShouldInsertOneRow()
    {
        await Act.CreateUser("alice", "alice@example.com");

        await Assert.CountIs(1);
    }

    [Fact]
    public async Task DeleteUser_ShouldRemoveExistingUser()
    {
        await Arrange.WithExistingUser("bob", "bob@example.com");

        await Act.DeleteUser(userId: 1);

        await Assert.CountIs(0);
    }
}
```

📌 **Key Points:**
- Each framework class has one responsibility: Arrange, Act, or Assert.
- Tests read as plain English — no infrastructure code, no URL strings, no SQL.
- Adding or changing a method in one framework class fixes every test that uses it.
- The base test class is the single point of change if the environment evolves.
- You can have multiple framework families (`UserArrange`/`UserAct`/`UserAssert`, `OrderArrange`/`OrderAct`/`OrderAssert`…) and mix them freely on the base class.

---

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
