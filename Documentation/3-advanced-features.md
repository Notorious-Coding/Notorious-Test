## Advanced Functionalities

- [Configuration](#configuration)
- [Web Testing](#web-testing)
- [Extensions](#extensions)
- [Dependency Injection](#dependency-injection)
- [Logging](#logging)
- [Miscellaneous](#miscellaneous)

### Configuration

In some cases, infrastructures need to produce configuration values — connection strings, secrets, ports, etc. NotoriousTest provides a typed, composable system to produce and consume configuration across infrastructures.

#### How it works

Each infrastructure can produce a list of `ConfigurationEntry<T>`, where `T` is the type of your choice. The environment aggregates all entries and passes them to consumers (e.g. a `WebApplication`) which are responsible for converting them to the format they need.

#### Producing configuration

To produce configuration, inherit from `Infrastructure<TOutputConfiguration, TMetadata>` and call `AddEntry(key, value)` inside `Initialize()`.
The key will be used by the consumer to determine where to place this section of the config.
e.g. `WebApplication` uses it as a path for appsettings replacement (`Key:Key:Key`).

```csharp
public class SqlServerInfrastructure : Infrastructure<LanaeDatabase, object>
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        // Start your container, get the connection string...
        AddEntry("Databases:Lanae", new LanaeDatabase
        {
            ConnectionString = "Server=localhost;..."
        });
    }

    public override Task Reset() => Task.CompletedTask;
    public override Task Destroy() => Task.CompletedTask;
}
```

If you don't need a typed output, use `Infrastructure<string, object>` and pass a plain string value:

```csharp
public class RedisInfrastructure : Infrastructure<string, object>
{
    public RedisInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override async Task Initialize()
    {
        AddEntry("Cache:Redis:Host", "localhost");
    }

    public override Task Reset() => Task.CompletedTask;
    public override Task Destroy() => Task.CompletedTask;
}
```

#### Consuming configuration from another infrastructure

An infrastructure can consume the configuration produced by previously initialized infrastructures by implementing `IConfigurationConsumer`.

```csharp
public class MyInfrastructure : Infrastructure, IConfigurationConsumer
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new();

    public override Task Initialize()
    {
        var redisHost = ConsumedConfiguration
            .FirstOrDefault(e => e.Key == "Cache:Redis:Host")?.Value?.ToString();

        // Use redisHost...
        return Task.CompletedTask;
    }

    public override Task Reset() => Task.CompletedTask;
    public override Task Destroy() => Task.CompletedTask;
}
```

> ❗ An infrastructure can only consume configuration from infrastructures that were initialized before it. Order of declaration in `ConfigureEnvironment` determines availability.

### Web Testing

#### Web Application

Create a `WebApplication<TEntryPoint>` to wrap your `WebApplicationFactory`:

```csharp
internal class SampleProjectApp : WebApplication<Program> { }
```

`WebApplication` automatically receives the aggregated configuration from all producer infrastructures and injects it as an `InMemoryCollection` into your app's `IConfiguration`.

#### Web Environment

```csharp
public class SampleEnvironment : NotoriousTest.XUnit.Environment
{
    public SampleEnvironment(IMessageSink sink) : base(sink) { }
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override Task ConfigureEnvironment()
    {
        AddInfrastructure<SqlServerInfrastructure>();
        AddInfrastructure<RedisInfrastructure>();
        this.AddWebApplication<SampleProjectApp>();

        return Task.CompletedTask;
    }
}
```

The `WebApplication` is always initialized last — it depends on the configuration produced by all other infrastructures.

In your tests, access the `HttpClient` via `GetWebApplication()`:

```csharp
[Fact]
public async Task ShouldReturnWeather()
{
    HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient!;

    HttpResponseMessage response = await client.GetAsync("api/weather");
    Assert.True(response.IsSuccessStatusCode);
}
```

### Extensions

Extensions let you hook into the infrastructure lifecycle without modifying the infrastructure itself. They implement `IInfrastructureExtension` and respond to events like `OnBeforeInitialize`, `OnAfterInitialize`, `OnBeforeReset`, etc.

#### Registering an extension

```csharp
public class MyInfrastructure : Infrastructure
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        EnsureExtension<MyCustomExtension>();
        // or with an instance:
        EnsureExtension(new MyCustomExtension());
    }
}
```

#### Built-in extensions

**`OutputConfigurationExtension<T>`** — used internally by `Infrastructure<TOutputConfiguration, TMetadata>` to produce configuration entries. You rarely need to use this directly.

**`SettingsExtension<TSettings>`** — loads settings from `testsettings.json` and exposes them via `extension.Settings`. The section name defaults to the infrastructure's class name.

```csharp
public class MySettings
{
    public string ConnectionString { get; set; }
}

public class MyInfrastructure : Infrastructure
{
    private SettingsExtension<MySettings> _settings;

    public MyInfrastructure(EnvironmentId contextId, ITestSettingsProvider settingsProvider, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        _settings = EnsureExtension(new SettingsExtension<MySettings>(settingsProvider));
    }

    public override Task Initialize()
    {
        var connectionString = _settings.Settings.ConnectionString;
        // use connection string...
        return Task.CompletedTask;
    }
}
```

**`RespawnExtension`** (from `NotoriousTest.Database`) — resets the database before each test using [Respawn](https://github.com/jbogard/Respawn). Used automatically by `SqlServerContainerInfrastructure` and `PostgreContainerInfrastructure`.

```csharp
EnsureExtension(new RespawnExtension(() => new RespawnerOptions
{
    TablesToIgnore = new Table[] { "Migrations" }
}));
```

### Dependency Injection

You can register services that will be injected into your infrastructures. Override `ConfigureInfrastructureServices` in your environment:

```csharp
public class SampleEnvironment : NotoriousTest.XUnit.Environment
{
    public SampleEnvironment(IMessageSink sink) : base(sink) { }
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override void ConfigureInfrastructureServices(IServiceCollection collection)
    {
        base.ConfigureInfrastructureServices(collection);
        collection.AddSingleton<IMyService, MyService>();
    }

    public override Task ConfigureEnvironment()
    {
        AddInfrastructure<MyInfrastructure>();
        return Task.CompletedTask;
    }
}
```

Infrastructure constructors will receive registered services automatically via `ActivatorUtilities`.

### Logging

Every infrastructure receives an `ITestLogger` via its constructor, accessible through the `Logger` property. Messages are forwarded to xUnit's output system.

Enable diagnostic messages in `xunit.runner.json`:

```json
{
  "diagnosticMessages": true
}
```

Then use `Logger` inside your infrastructure:

```csharp
public override async Task Initialize()
{
    Logger.Log("Starting my infrastructure...");
    // ...
}
```

Log output appears in Visual Studio's test output window or the terminal when running `dotnet test`.

### Miscellaneous

#### Ordering infrastructure execution

Use the `Order` property to control initialization, reset, and teardown sequence:

```csharp
public class SqlServerInfrastructure : Infrastructure
{
    public override int? Order => 1;
}
```

Infrastructures without an `Order` are executed first by default. `WebApplicationInfrastructure` is always executed last (order 999).

#### Disabling automatic reset

By default, all infrastructures are reset between each test. You can disable this per infrastructure:

```csharp
public class SampleEnvironment : NotoriousTest.XUnit.Environment
{
    public SampleEnvironment(IMessageSink sink) : base(sink) { }
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override Task ConfigureEnvironment()
    {
        AddInfrastructure<SqlServerInfrastructure>();
        GetInfrastructure<SqlServerInfrastructure>().AutoReset = false;
        this.AddWebApplication<SampleProjectApp>();

        return Task.CompletedTask;
    }
}
```

Alternatively, set `AutoReset` directly in the infrastructure constructor:

```csharp
public class MyInfrastructure : Infrastructure
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry)
    {
        AutoReset = false;
    }
}
```

> ❗ Use this carefully — tests must not depend on leftover state unless explicitly intended.
