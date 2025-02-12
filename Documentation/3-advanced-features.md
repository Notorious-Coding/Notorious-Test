## Advanced functionalities

- [Configuration](#configuration)
- [Web Testing](#web-testing)
- [Miscellenaous](#miscellenaous)

### Configuration

In some cases, you will need to handle configuration from your infrastructures, such as connection string, or secrets, etc. **Notorious Tests** provides a nice way to produce and consume configuration within infrastructures.

Here's how :

#### Configurable Infrastructures

To create a configurable infrastructure, the only thing you need to do is to inherits from **ConfiguredInfrastructure** or **AsyncConfiguredInfrastructure**.

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

As you can see, this infrastructure will produce configuration after initializing the sql server.
**Configuration** is available as public so you can then access it when using this infrastructure.

Generic type for configuration is not mandatory, Configuration object will be a **Dictionary<string, string>**

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

Now, you can access your infrastructure configuration directly within a test :

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

You could use **`ConfiguredInfrastructure`** within **Environment**.

First, inherit from **`ConfiguredEnvironment`** instead of **Environment**.
(from **AsyncConfiguredEnvironment** instead of **AsyncEnvironment**).

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

Within any **`ConfiguredEnvironment`**, you would use the public property **`EnvironmentConfiguration`** to access produced configuration from infrastructures.

> ❗ Inside an environment, **configuration object should represent the whole app configuration**. It will move from infrastructures to infrastructures, and each infrastructure will edit its own part of the configuration.

### Web Testing

Within web applications, you will certainly need to add a WebApplication that work in background so your tests can call an actual API.

**NotoriousTests** provide everything so you donc need to scratch your head too much.

#### Web Application Infrastructure

First, you will need to create an **WebApplication**.

```csharp
internal class SampleProjectApp : WebApplication<Program>{}
```

This is actually a **WebApplicationFactory** provided by .NET ([See microsoft doc for more information.](https://learn.microsoft.com/fr-fr/aspnet/core/test/integration-tests?view=aspnetcore-8.0))

Here you can override everything you need for your app to be fully functional.

Then, let's create an **WebApplicationInfrastructure** and pass our **WebApplication**.

> :information: Within a **WebEnvironment**, creating an infrastructure is optional. You can directly pass a WebApplication. But this can be usefull to add initialization/reset/destroy behaviors.

```csharp
    internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program, Configuration>
    {
        public SampleProjectWebApplicationInfrastructure(Dictionary<string, string> configuration)
            : base(new SampleProjectApp())
        {
        }
    }

    // Without a generic type, configuration will be a Dictionary<string, string>.
    internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program>
    {
        public SampleProjectWebApplicationInfrastructure(Dictionary<string, string> configuration)
            : base(new SampleProjectApp())
        {
        }
    }
```

> :information: Configuration passed will be automaticaly added as configuration on your web app, making it accessible within **IConfiguration** object in _Program.cs_.

Now, within your test, you can start your **WebApplicationInfrastructures** like any other infrastructure, and access **HttpClient**:

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

**WebApplication** or **WebApplicationInfrastructure** can be used within **WebEnvironment**.

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

Your web application will start automatically at the start of a test campaign, and configuration produced by infrastructures will automaticaly be added as an **InMemoryCollection** to your WebApplication.

Then, in your test, you can use **GetWebApplication** to access the HttpClient :

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

Nice ! Good job, now, your integration test are isolated from each other.

### Miscellenaous

#### Ordering infrastructures execution

In certain contexts, the execution order of infrastructures within an environment can be crucial.

To address this, all infrastructures provide an optional `Order` property. This allows specifying the order of initialization, reset, and teardown of infrastructures.

By default, infrastructures without a defined order will be prioritized and executed first.

> ❗ The only exception is WebApplicationInfrastructure, which will ALWAYS be executed last, as it depends on other infrastructures' configuration.

Here's an example :

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

When `AutoReset` is set to `false`, the `Reset` method of the specified infrastructure will be skipped during the test lifecycle. This can save significant time and resources in scenarios where resetting is unnecessary.

> :warning: Note: Use this option carefully. Ensure that tests are designed to avoid dependencies on leftover data unless explicitly intended.
