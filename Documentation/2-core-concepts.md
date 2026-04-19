# 🏗️ Core Concepts

NotoriousTest provides a structured way to manage **test infrastructures** and ensure **clean, isolated integration tests**.
This document introduces the core concepts and how they work together.

## Summary

- [Infrastructure](#-what-is-an-infrastructure)
- [Extensions](#-infrastructure-extensions)
  - [SettingsExtension](#settingsextension)
  - [OutputConfigurationExtension](#outputconfigurationextension)
  - [Custom Extensions](#custom-extensions)
- [Configuration Propagation](#️-configuration-propagation)
- [Test Environment](#-what-is-a-test-environment)
  - [Dependency Injection](#dependency-injection)
  - [Output Configuration & Web](#output-configuration--web)
- [Lifecycle](#-how-does-the-test-lifecycle-work)
- [Logging](#-logging)
- [DoggyDog](#-doggydog)
- [Accessing Infrastructures in Tests](#-how-to-access-infrastructures-in-tests)

---

## 🏗️ What is an Infrastructure?

An **infrastructure** represents an external dependency required for testing.
This could be a **database, message queue, file storage, external API**, or any service that needs to be set up and managed during tests.

```csharp
public class MyInfrastructure : Infrastructure
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override Task Initialize()
    {
        // Called once at the start of the test session
        return Task.CompletedTask;
    }

    public override Task Reset()
    {
        // Called before each test — restore a clean state
        return Task.CompletedTask;
    }

    public override Task Destroy()
    {
        // Called at the end of the session — release resources
        return Task.CompletedTask;
    }
}
```

📌 **Key Points:**

- `Initialize()` → Called once at the start of the test session.
- `Reset()` → Called before each test to ensure data isolation.
- `Destroy()` → Called at the end of the test session to clean up resources.
- The constructor receives `EnvironmentId`, `ITestLogger`, and `IRegistry` — all injected automatically by the DI container.

> **Note:** All `Async`-prefixed classes (`AsyncInfrastructure`, `AsyncEnvironment`, etc.) have been removed in v4.0.0. Use `Infrastructure`, `Environment`, `IntegrationTest` directly.

---

## 🧩 Infrastructure Extensions

**Extensions** introduce a composition model for adding reusable behaviors to your infrastructures without subclassing. Instead of creating specialized subclasses for every concern (configuration output, database seeding, settings loading...), you register extensions via `EnsureExtension<T>()`.

Each extension hooks into the infrastructure lifecycle through dedicated callbacks:

```csharp
public interface IInfrastructureExtension
{
    Task OnBeforeInitialize(IInfrastructure infrastructure);
    Task OnAfterInitialize(IInfrastructure infrastructure);
    Task OnBeforeReset(IInfrastructure infrastructure);
    Task OnAfterReset(IInfrastructure infrastructure);
    Task OnBeforeDestroy(IInfrastructure infrastructure);
    Task OnAfterDestroy(IInfrastructure infrastructure);
}
```

Register an extension from within an infrastructure's constructor or `Initialize()`:

```csharp
public class MyInfrastructure : Infrastructure
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry,
                            ITestSettingsProvider settings)
        : base(contextId, logger, registry)
    {
        // Register a built-in extension
        EnsureExtension(new SettingsExtension<MySettings>(settings));
    }
}
```

`EnsureExtension<T>()` is idempotent: if an extension of type `T` is already registered, the existing instance is returned.

---

### SettingsExtension

`SettingsExtension<TSettings>` loads configuration from a `testsettings.json` file into a typed settings object, **before** `Initialize()` runs.

**1. Create a `testsettings.json`** in your test project and set `Copy to Output Directory: PreserveNewest`:

```json
{
  "MyInfrastructure": {
    "Host": "localhost",
    "Port": 1433
  }
}
```

**2. Define a settings class:**

```csharp
public class MySettings
{
    public string Host { get; set; }
    public int Port { get; set; }
}
```

**3. Register the extension in your infrastructure:**

```csharp
public class MyInfrastructure : Infrastructure
{
    private SettingsExtension<MySettings> _settings;

    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry,
                            ITestSettingsProvider settingsProvider)
        : base(contextId, logger, registry)
    {
        _settings = EnsureExtension(new SettingsExtension<MySettings>(settingsProvider));
    }

    public override Task Initialize()
    {
        // Settings are loaded before Initialize() is called
        var host = _settings.Settings.Host;
        var port = _settings.Settings.Port;
        return Task.CompletedTask;
    }
}
```

📌 **Key Points:**
- The section key defaults to the infrastructure's class name. You can override it via the `SectionName` property.
- `ITestSettingsProvider` is automatically registered in the DI container by the environment.

---

### OutputConfigurationExtension

`OutputConfigurationExtension<TConfig>` allows an infrastructure to **expose configuration** (e.g., a connection string, a base URL) that other infrastructures or the web application can consume.

It is built into the `Infrastructure<TOutputConfiguration, TMetadata>` base class, so you don't need to register it manually when you use that base class.

**Example:** an infrastructure that outputs a connection string:

```csharp
public class MyDatabaseInfrastructure : Infrastructure<ConnectionStringConfig, object>
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        // ... start database ...
        string connectionString = "Server=localhost;Database=...";

        // Expose a configuration entry — the key is free-form, interpreted by the consumer
        AddEntry("ConnectionStrings:MyDb", new ConnectionStringConfig(connectionString));
    }
}
```

📌 **Key Points:**
- The key is a free-form string — its meaning is entirely determined by the **consumer**.
- The environment collects all `ConfigurationEntry` objects and passes them as a flat list to every `IConfigurationConsumer` infrastructure.
- **`WebApplicationInfrastructure`** (from `NotoriousTest.Web`) is the built-in consumer: it interprets keys as appsettings paths and maps entries accordingly:

```json
// Entry key: "Example:Database" → value: { "Host": "localhost", "Port": 5432 }
// Injected into the web app as:
{
  "Example": {
    "Database": {
      "Host": "localhost",
      "Port": 5432
    }
  }
}
```

Any infrastructure implementing `IConfigurationConsumer` receives the same list and can interpret the keys however it needs.

---

### Custom Extensions

An extension is a self-contained class that encapsulates a specific, reusable behavior. The canonical use case is to **strongly type it against the infrastructure it targets** using `IInfrastructureExtension<T>`, giving it direct access to all infrastructure members.

**Example:** a seeding extension that runs reference data inserts after a database is initialized:

```csharp
public class ReferenceDataSeedExtension : IInfrastructureExtension<MyDatabaseInfrastructure>
{
    public async Task OnAfterInitialize(MyDatabaseInfrastructure infrastructure)
    {
        using var connection = infrastructure.GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Countries (Code, Name) VALUES ('FR', 'France');
            INSERT INTO Countries (Code, Name) VALUES ('DE', 'Germany');
            INSERT INTO Countries (Code, Name) VALUES ('US', 'United States');
        ";
        await command.ExecuteNonQueryAsync();
    }
}
```

Register it in your infrastructure:

```csharp
public class MyDatabaseInfrastructure : Infrastructure
{
    public MyDatabaseInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        EnsureExtension<ReferenceDataSeedExtension>();
    }
}
```

📌 **Key Points:**
- The extension has its own state and dependencies — it is a proper class, not a delegate wrapper.
- `IInfrastructureExtension<T>` gives the extension typed access to the infrastructure without casting.
- Any hook not overridden has a default no-op implementation — only implement what you need.

---

## ⚙️ Configuration Propagation

`OutputConfigurationExtension` and `Infrastructure<TConfig, TMetadata>` are built on top of two simple interfaces that define how infrastructures communicate configuration to each other:

```csharp
// Produces configuration entries for other infrastructures
public interface IConfigurationProducer
{
    List<ConfigurationEntry<object>> OutputConfiguration { get; }
}

// Receives the aggregated configuration entries from all producers
public interface IConfigurationConsumer
{
    List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; }
}
```

The environment orchestrates the propagation automatically:
1. After all infrastructures are registered, it collects every `ConfigurationEntry` from every `IConfigurationProducer`.
2. Before initializing any `IConfigurationConsumer`, it injects the full aggregated list into `ConsumedConfiguration`.
3. `IConfigurationConsumer` infrastructures always initialize **last within their order group**, so producers are guaranteed to be ready first.

This means you can implement `IConfigurationConsumer` directly on any infrastructure that needs to read entries produced by others — not just `WebApplicationInfrastructure`:

```csharp
public class MyConsumerInfrastructure : Infrastructure, IConfigurationConsumer
{
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = [];

    public override Task Initialize()
    {
        var entry = ConsumedConfiguration.FirstOrDefault(e => e.Key == "ConnectionStrings:MyDb");
        // use entry.Value ...
        return Task.CompletedTask;
    }
}
```

---

## 🌍 What is a Test Environment?

A test environment is a collection of infrastructures that define the full setup for running tests. It manages the DI container, lifecycle, and DoggyDog watchdog.

Extend the class from your framework-specific package:

```csharp
// xUnit
public class MyTestEnvironment : NotoriousTest.XUnit.Environment
{
    public MyTestEnvironment(IMessageSink sink) : base(sink) { }

    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        // Infrastructures are created via DI — constructor parameters are injected automatically
        AddInfrastructure<MyInfrastructure>();
        AddInfrastructure<MyOtherInfrastructure>();
    }
}
```

| Framework | Base class |
|-----------|-----------|
| xUnit | `NotoriousTest.XUnit.Environment` |
| NUnit | `NotoriousTest.NUnit.Environment` |
| MSTest | `NotoriousTest.MSTest.Environment` |
| TUnit | `NotoriousTest.TUnit.Environment` |

📌 **Key Points:**

- `AddInfrastructure<T>()` resolves `T` through the DI container — all constructor parameters are injected.
- `AddInfrastructure(new MyInfrastructure(...))` can be used to pass a manually created instance.
- Infrastructures run in parallel when they share the same `Order` value.
- `IConfigurationConsumer` always run in last in their order group.

### Dependency Injection

Environments expose an internal DI container. Override `ConfigureInfrastructureServices` to register your own services, which will then be injected into any infrastructure constructor:

```csharp
public class MyTestEnvironment : NotoriousTest.XUnit.Environment
{
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override void ConfigureInfrastructureServices(IServiceCollection services)
    {
        base.ConfigureInfrastructureServices(services); // always call base

        // Register custom services — available in all infrastructure constructors
        services.AddSingleton<IMyService, MyService>();
        services.AddSingleton<MyOptions>(new MyOptions { ... });
    }

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<MyInfrastructure>(); // receives IMyService via constructor injection
    }
}
```

**Available by default (no registration needed):**
- `EnvironmentId` — unique identifier for this test session
- `ITestSettingsProvider` — loads `testsettings.json`
- `ITestLogger` — framework-specific logger
- `IRegistry` — infrastructure registry (used by DoggyDog)

---

### Output Configuration & Web

When using `NotoriousTest.Web`, call `this.AddWebApplication<T>()` to register your web application. It automatically receives all configuration entries produced by other infrastructures:

```csharp
public override async Task ConfigureEnvironment()
{
    AddInfrastructure<MyDatabaseInfrastructure>(); // produces connection strings
    this.AddWebApplication<MyTestWebApp>();         // consumes them automatically
}
```

---

## 🔄 How Does the Test Lifecycle Work?

NotoriousTest manages your test lifecycle automatically to ensure clean and isolated tests.

```
Test Session Starts
  └─ DI container is built
  └─ Registry is set up
  └─ DoggyDog watchdog is launched
  └─ ConfigureEnvironment() is called
  └─ Infrastructures are initialized (Initialize()), in Order, in parallel when Order is equal

Before Each Test
  └─ Infrastructures are reset (Reset())

Test Runs
  └─ Test executes in an isolated environment

After Test Suite Completes
  └─ Infrastructures are destroyed (Destroy())
  └─ DoggyDog receives success signal — no cleanup needed
```

🔥 **Why is this important?**

✅ Guarantees clean data for each test (no unwanted side effects).\
✅ Prevents state leaks between tests.\
✅ Eliminates manual setup/teardown code in test classes.

**Controlling initialization order:**

```csharp
public class MyInfrastructure : Infrastructure
{
    public override int? Order => 1; // lower runs first; null = unordered
}
```

**Disabling reset for a specific infrastructure:**

```csharp
public class MyInfrastructure : Infrastructure
{
    public override bool AutoReset => false; // Reset() will never be called
}
```

---

## 📋 Logging

Every infrastructure has access to a `Logger` property of type `ITestLogger`, injected automatically. It routes log output to the test framework's native output (console, test output window, etc.).

```csharp
public class MyInfrastructure : Infrastructure
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override Task Initialize()
    {
        Logger.Log("Initializing MyInfrastructure...");
        return Task.CompletedTask;
    }
}
```

`ITestLogger` is also registered in the DI container, so you can inject it into any service registered via `ConfigureInfrastructureServices`.

---

## 🐶 DoggyDog

**DoggyDog** is a watchdog process that cleans up infrastructures left in a dirty state when a test campaign is killed unexpectedly (crash, forced termination, IDE stop, etc.).

### How it works

1. When a test session starts, the environment launches DoggyDog as a child process.
2. Every infrastructure registers itself in a local SQLite registry when it initializes.
3. DoggyDog monitors the test process. If the process exits without sending a success signal, DoggyDog runs the **cleaner** for each registered infrastructure.
4. On a clean exit, the environment sends a success signal and DoggyDog shuts down without doing anything.

To locate cleaner implementations on crash recovery, DoggyDog scans the test assembly via reflection. This is why every environment requires a `CurrentAssembly` property:

```csharp
public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();
```

DoggyDog uses this assembly to resolve the `[Cleaner(typeof(T))]` types at runtime. Without it, cleaners would not be found and crash recovery would be a no-op.

### Defining a Cleaner

Annotate your infrastructure with `[Cleaner(typeof(T))]` and implement `IInfrastructureCleaner<TMetadata>`:

```csharp
[Cleaner(typeof(MyInfrastructureCleaner))]
public class MyInfrastructure : Infrastructure<object, MyMetadata>
{
    public override async Task Initialize()
    {
        // Store cleanup metadata so DoggyDog can clean up if we crash
        Metadata = new MyMetadata { ContainerName = "my-container" };
    }
}

public class MyInfrastructureCleaner : IInfrastructureCleaner<MyMetadata>
{
    public async Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, MyMetadata? metadata)
    {
        if (metadata is null) return;
        // e.g., stop and remove a Docker container
        await StopContainer(metadata.ContainerName);
    }
}
```

📌 **Key Points:**

- `Metadata` is serialized to the registry when the infrastructure initializes and passed back to the cleaner on crash recovery.
- If an infrastructure does not need crash cleanup, no `[Cleaner]` attribute is needed.
- Built-in infrastructures (SqlServer, PostgreSql, Sqlite via TestContainers) already implement their own cleaners.

### Logging

DoggyDog now logs infrastructure lifecycle events during normal test execution — not just on crash recovery. You will see log entries when an infrastructure is **initialized**, **reset**, or **destroyed** as part of a clean test run.

### Configuration

DoggyDog can be configured via your `testsettings.json` file.

#### Disabling DoggyDog

To disable DoggyDog for the entire test project, set `DisableWatchdog` to `true` under the `Environment` section:

```json
{
  "Environment": {
    "DisableWatchdog": true
  }
}
```

All registry operation and doggydog start will be skip.

#### Manual Launch (debugging)

For debugging purposes, you can instruct the test suite to wait for DoggyDog to be launched manually instead of spawning it automatically. Set `ManualLaunch` to `true` under the `Watchdog` section:

```json
{
  "Watchdog": {
    "ManualLaunch": true
  }
}
```

When `ManualLaunch` is enabled, the environment will publish the required parameters as **User Environment Variables**. You can then start DoggyDog manually with the `--from-env` flag:

```sh
DoggyDog --from-env
```

This lets you attach a debugger to DoggyDog or inspect its behavior before the test campaign proceeds.

---

## 🔌 How to Access Infrastructures in Tests

Once a test environment is configured, retrieve any registered infrastructure inside your tests via `GetInfrastructure<T>()`:

```csharp
// xUnit
public class MyIntegrationTests : NotoriousTest.XUnit.IntegrationTest<MyTestEnvironment>
{
    public MyIntegrationTests(MyTestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task ExampleTest()
    {
        var infra = CurrentEnvironment.GetInfrastructure<MyInfrastructure>();

        Assert.NotNull(infra);
    }
}
```

📌 **Key Takeaways:**

- `GetInfrastructure<T>()` — retrieves an infrastructure by type. Throws if not found.
- No async needed — infrastructure retrieval is synchronous.
- Tests remain clean and focused on logic instead of infrastructure handling.

---

## ✅ Summary

| Concept | Description |
|---------|-------------|
| Concept | Description |
|---------|-------------|
| `Infrastructure` | Base class for all external dependencies (DB, API, queue…). |
| `IInfrastructureExtension` | Lifecycle hooks to add reusable behaviors to any infrastructure. |
| `SettingsExtension<T>` | Loads typed settings from `testsettings.json` before `Initialize()`. |
| `OutputConfigurationExtension<T>` | Publishes configuration entries for other infrastructures to consume. |
| `IConfigurationProducer` | Contract for infrastructures that publish configuration entries. |
| `IConfigurationConsumer` | Contract for infrastructures that receive the aggregated entries. |
| `Environment` | Groups infrastructures, owns the DI container and lifecycle. |
| `ConfigureInfrastructureServices` | Override to register custom services injected into infrastructure constructors. |
| `ITestLogger` | Framework-native logging, injected via `Infrastructure.Logger`. |
| DoggyDog | Watchdog process that cleans up after unexpected test crashes. |
| `[Cleaner(typeof(T))]` | Designates the crash-recovery handler for an infrastructure. |
| `GetInfrastructure<T>()` | Retrieves a registered infrastructure inside a test. |

---

Now that you understand the fundamentals, check out:

- 🔌 [Supported Infrastructures](./3-integrations.md) – See how to integrate SQL Server, TestContainers, and more.
- 📚 [Examples](./4-example.md) – Hands-on use cases with real-world setups.
- 🏛️ [Architecture Guidelines](./5-architecture.md) – Best practices for structuring your test setup.

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
