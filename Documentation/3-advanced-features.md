## Advanced functionalities

- [Configuration](#configuration)
- [Web Testing](#web-testing)
- [Miscellenaous](#miscellenaous)

### Configuration

In some cases, you may need to manage configurations from your infrastructure, such as connection strings, secrets, and other settings. Notorious Tests provides a seamless way to generate and consume configurations within your infrastructure.

Here’s how it works:

#### Configurable Infrastructures

To create a configurable infrastructure, simply inherit from **`ConfiguredInfrastructure`** or **`AsyncConfiguredInfrastructure`**.

```csharp
    public class DatabaseInfrastructure : ConfiguredInfrastructure<Configuration>
    {

        public DatabaseInfrastructure(bool initialize = false) : base(initialize)
        {
        }

        public override int Order => 1;

        public override void Destroy(){}

        public override void Initialize()
        {
            /// Initialize sql server.
            Configuration.DatabaseConfiguration = new DatabaseConfiguration()
            {
                ConnectionString = "Test"
            };
        }

        public override void Reset(){}
    }
```

As you can see, this infrastructure generates a configuration after initializing the SQL Server.
The **`Configuration`** property is publicly accessible, allowing you to retrieve it when using this infrastructure.

Specifying a generic type for the configuration is optional. By default, the **`Configuration`** object is a **`Dictionary<string, string>`**.

```csharp
    public class DatabaseInfrastructure : ConfiguredInfrastructure
    {

        public DatabaseInfrastructure(bool initialize = false) : base(initialize)
        {
        }

        public override int Order => 1;

        public override void Destroy(){}

        public override void Initialize()
        {
            /// Initialize sql server.
            Configuration.Add("DatabaseConfiguration.ConnectionString", "Test");
        }

        public override void Reset(){}
    }
```

Now, you can directly access your infrastructure's configuration within a test:

```csharp
[Fact]
public async Task Test2()
{
    await using (var db =  new DatabaseInfrastructure(initialize: true))
    {
        var cs = db.Configuration.DatabaseConfiguration.ConnectionString;
    }
}
```

#### Configurable Environment

You could use **`ConfiguredInfrastructure`** within an **`Environment`**.

To do this, inherit from **`ConfiguredEnvironment`** instead of **`Environment`** (or from **`AsyncConfiguredEnvironment`** instead of **`AsyncEnvironment`**).

```csharp
public class SampleEnvironment : ConfiguredEnvironment<Configuration>
{
    public override void ConfigureEnvironment()
    {
        // Add all your infrastructure here.
        AddInfrastructure(new DatabaseInfrastructure());
    }
}
// Or without generic type
public class SampleEnvironment : ConfiguredEnvironment{
    public override void ConfigureEnvironment()
    {
        // Add all your infrastructure here.
        AddInfrastructure(new DatabaseInfrastructure());
    }
}
```

In a **`ConfiguredEnvironment`**, you can access the generated configuration from infrastructures using the public **`EnvironmentConfiguration`** property.

> ❗ Inside an environment, **configuration object should represent the whole app configuration**. It will move from infrastructures to infrastructures, and each infrastructure will edit its own part of the configuration.

### Web Testing

When working with web applications, you'll often need to run a WebApplication in the background so your tests can interact with a real API.

**NotoriousTests** provides everything you need, so you don't have to overthink it.

#### Web Application Infrastructure

First, you need to create an **WebApplication**.

```csharp
internal class SampleProjectApp : WebApplication<Program>{}
```

This is essentially a WebApplicationFactory, a built-in .NET feature. ([See microsoft doc for more information.](https://learn.microsoft.com/fr-fr/aspnet/core/test/integration-tests?view=aspnetcore-8.0))

Here, you can override anything necessary to make your app fully functional.

Next, let's create a **`WebApplicationInfrastructure`** and pass our **`WebApplication`** to it.

> :information: In a **`WebEnvironment`**, creating a dedicated infrastructure is optional—you can pass a **`WebApplication`** directly. However, using an infrastructure can be useful if you need additional initialization, reset, or teardown logic.

```csharp
    internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program, Configuration>
    {
        public SampleProjectWebApplicationInfrastructure()
            : base(new SampleProjectApp())
        {
        }
    }

    // Without a generic type, configuration will be a Dictionary<string, string>.
    internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program>
    {
        public SampleProjectWebApplicationInfrastructure()
            : base(new SampleProjectApp())
        {
        }
    }
```

> :information: The provided configuration will automatically be applied to your web app, making it accessible through the **`IConfiguration`** object in Program.cs.

Now, in your tests, you can start your **`WebApplicationInfrastructure`** just like any other infrastructure and access the `HttpClient`:

```csharp
[Fact]
public async Task Test2()
{
    await using (var app = new SampleProjectWebApplicationInfrastructure())
    {
        HttpClient? client = app.HttpClient;

        HttpResponseMessage response = await client!.GetAsync("api/weather");
        Assert.True(response.IsSuccessStatusCode);

        string content = await response.Content.ReadAsStringAsync();
    }
}
```

#### Web Environment

You can use either **`WebApplication`** or **`WebApplicationInfrastructure`** inside a **`WebEnvironment`**.

```csharp
    public class SampleEnvironment : AsyncWebEnvironment<Program, Configuration>
    {
        public override Task ConfigureEnvironmentAsync()
        {
            AddInfrastructure(new DatabaseInfrastructure());
            AddWebApplication(new SampleProjectApp());
            // OR
            AddWebApplication(new SampleProjectWebApplicationInfrastructure());

            return Task.CompletedTask;
        }
    }
```

Your web application will start automatically at the beginning of a test campaign, and any configuration generated by infrastructures will be added as an `InMemoryCollection` to your **`WebApplication`**.

Then, in your tests, you can use **`GetWebApplication`** to access the `HttpClient`:

```csharp
[Fact]
public async Task Test2()
{
    HttpClient? client = (await CurrentEnvironment.GetWebApplication()).HttpClient;

    HttpResponseMessage response = await client!.GetAsync("api/weather");
    Assert.True(response.IsSuccessStatusCode);

    string content = await response.Content.ReadAsStringAsync();
}
```

Nice! Well done—your integration tests are now fully isolated from each other.

### Miscellenaous

#### Ordering infrastructures execution

In some cases, the execution order of infrastructures within an environment is critical.

To handle this, all infrastructures include an optional **`Order`** property, allowing you to define the sequence for initialization, reset, and teardown.

By default, infrastructures without a specified order are prioritized and executed first.

❗ The only exception is **`WebApplicationInfrastructure`**, which is ALWAYS executed last, as it depends on configurations from other infrastructures.

Here's an example:

```csharp
    public class DatabaseInfrastructure : AsyncConfiguredInfrastructure<Configuration>
    {
        // Use this property to order execution.
        public override int? Order => 1;
        public DatabaseInfrastructure(bool initialize = false): base(initialize)
        {
        }

        public override Task Destroy()
        {
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }
```

#### Advanced Control Over Infrastructure Resets

By default, infrastructures are reset between each test to ensure data isolation. However, in certain scenarios -such as multi-tenant applications where isolation is ensured by design- automatic resets may not be necessary.

**With the `AutoReset` option, you can disable the automatic reset for a specific infrastructure:**

```csharp
    public class SampleEnvironment : AsyncWebEnvironment<Program, Configuration>
    {
        public override Task ConfigureEnvironmentAsync()
        {
            AddInfrastructure(new DatabaseInfrastructure()
            {
                AutoReset = false
            });
            AddWebApplication(new SampleProjectApp());

            return Task.CompletedTask;
        }
    }
```

When **`AutoReset`** is set to `false`, the **`Reset`** method of the specified infrastructure will be skipped during the test lifecycle. This can save significant time and resources in scenarios where resetting is unnecessary.

> ❗ Note: Use this option carefully. Ensure that tests are designed to avoid dependencies on leftover data unless explicitly intended.
