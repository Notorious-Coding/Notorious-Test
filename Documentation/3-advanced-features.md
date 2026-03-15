## Advanced functionalities

- [Configuration](#configuration)
- [Web Testing](#web-testing)
- [Miscellaneous](#miscellaneous)

### Configuration

In some cases, infrastructures need to produce configuration values — connection strings, secrets, ports, etc. NotoriousTest provides a typed, composable system to produce and consume configuration across infrastructures.

#### How it works

Each infrastructure can produce a list of `ConfigurationEntry<T>`, where `T` is the type of your choice. The environment aggregates all entries and passes them to consumers (e.g. a `WebApplication`) which are responsible for converting them to the format they need.

#### Producing configuration

To produce configuration, call `AddOutputConfigurationEntry` inside `Initialize()`.
The key will be used by the consumer to determine where to place this section of the config.
e.g. `WebApplication` uses it as a path for appsettings replacement (`Key:Key:Key`).

```csharp
public class SqlServerInfrastructure : Infrastructure<LanaeDatabase>
{
    public override async Task Initialize()
    {
        // Start your container, get the connection string...
        AddOutputConfigurationEntry("Databases:Lanae", new LanaeDatabase
        {
            ConnectionString = "Server=localhost;..."
        });
    }

    public override Task Reset() => Task.CompletedTask;
    public override Task Destroy() => Task.CompletedTask;
}
```

If you don't need a typed object, `Infrastructure` (without generic) accepts `object` directly:

```csharp
public class RedisInfrastructure : Infrastructure
{
    public override async Task Initialize()
    {
        AddOutputConfigurationEntry("Cache:Redis:Host", "localhost");
    }
}
```

#### Consuming configuration from another infrastructure

An infrastructure can consume the configuration produced by previously initialized infrastructures by implementing `IConfigurationConsumer`.

```csharp
public class MyInfrastructure : Infrastructure, IConfigurationConsumer
{
    public List<ConfigurationEntry<object>> ConsumedConfiguration { get; set; } = new();

    public override Task Initialize()
    {
        var redisHost = ConsumedConfiguration
            .FirstOrDefault(e => e.Key == "Cache:Redis:Host")?.Value?.ToString();

        // Use redisHost...
        return Task.CompletedTask;
    }
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
public class SampleEnvironment : AsyncWebEnvironment
{
    public override Task ConfigureEnvironmentAsync()
    {
        AddInfrastructure(new SqlServerInfrastructure());
        AddInfrastructure(new RedisInfrastructure());
        AddWebApplication(new SampleProjectApp());

        return Task.CompletedTask;
    }
}
```

The `WebApplication` is always initialized last — it depends on the configuration produced by all other infrastructures.

In your tests, access the `HttpClient` via `GetWebApplication`:

```csharp
[Fact]
public async Task ShouldReturnWeather()
{
    HttpClient client = (await CurrentEnvironment.GetWebApplication()).HttpClient!;

    HttpResponseMessage response = await client.GetAsync("api/weather");
    Assert.True(response.IsSuccessStatusCode);
}
```

### Miscellaneous

#### Ordering infrastructure execution

Use the `Order` property to control initialization, reset, and teardown sequence:

```csharp
public class SqlServerInfrastructure : Infrastructure
{
    public override int? Order => 1;
}
```

Infrastructures without an `Order` are executed first by default. `WebApplicationInfrastructure` is always executed last.

#### Disabling automatic reset

By default, all infrastructures are reset between each test. You can disable this per infrastructure:

```csharp
public class SampleEnvironment : AsyncWebEnvironment
{
    public override Task ConfigureEnvironmentAsync()
    {
        AddInfrastructure(new SqlServerInfrastructure { AutoReset = false });
        AddWebApplication(new SampleProjectApp());

        return Task.CompletedTask;
    }
}
```

> ❗ Use this carefully — tests must not depend on leftover state unless explicitly intended.