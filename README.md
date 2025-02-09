## ![Logo](./Documentation/Images/NotoriousTest.png)

[![NuGet](https://img.shields.io/nuget/v/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![License](https://img.shields.io/github/license/Notorious-Coding/Notorious-Test)](https://github.com/Notorious-Coding/Notorious-Test/blob/master/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-6%2B-blue)](https://dotnet.microsoft.com/)
[![GitHub stars](https://img.shields.io/github/stars/Notorious-Coding/Notorious-Test?style=social)](https://github.com/Notorious-Coding/Notorious-Test/stargazers)

**Notorious Test** provide a simple way to isolate integration tests. Based on XUnit.

## Contact

Have questions, ideas, or feedback about NotoriousTests?
Feel free to reach out! I'd love to hear from you. Here's how you can get in touch:

- GitHub Issues: [Open an issue](https://github.com/Notorious-Coding/Notorious-Test/issues) to report a problem, request a feature, or share an idea.
- Email: [briceschumacher21@gmail.com](mailto:briceschumacher21@gmail.com)
- LinkedIn : [Brice SCHUMACHER](http://www.linkedin.com/in/brice-schumacher)

The discussions tabs is now opened !
Feel free to tell me if you use the package here : https://github.com/Notorious-Coding/Notorious-Test/discussions/1 !

## Summary

- [Support](#support)
- [Features](#features)
- [Motivation](#motivation)
- [Changelog](#changelog)
- [Getting started](#getting-started)
- [Setup](#setup)
- [Base functionalities](#base-functionalities)
  - [Infrastructures](#infrastructures)
  - [Environment](#environment)
- [Advanced functionalities](#advanced-functionalities)
  - [Ordering infrastructures execution](#ordering-infrastructures-execution)
  - [Advanced Control Over Infrastructure Resets](#advanced-control-over-infrastructure-resets)
  - [Configuration](#configuration)
    - [Configurable Infrastructures](#configurable-infrastructures)
    - [Configurable Environment](#configurable-environment)
  - [Web](#web)
    - [Web Application Infrastructure](#web-application-infrastructure)
    - [Web Environment](#web-environment)
  - [TestContainers](#testcontainers)
  - [Sql Server](#sql-server)
    - [Initialization](#initialization)
    - [Test Usage](#test-usage)
    - [Populating the database](#populating-the-database)
    - [Generating configuration](#generating-configuration)
    - [Configure the container](#configure-the-container)
    - [Configure respawn settings](#configure-respawn-settings)
    - [Configure the database name](#configure-the-database-name)
- [Hand's On examples](#hands-on-examples)

## Support

- Net6+

## Features

- Easy share of test infrastructure.
- Easy building of test framework.
- Complete isolation of integration tests.
- Simple implementation.
- Based on XUnit.

## Motivation

The goal is to provide a way to make integration tests without worrying about data collision, side effects, and infrastructure sharing.

## Changelog

You can find the changelog [here](./CHANGELOG.md).

## Setup

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [NotoriousTest](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest
```

## Base functionalities

### Infrastructures

An infrastructure is a piece of hardware or software that is necessary for your app to work.
For example, a SQL Server Database is an infrastructure.
**NotoriousTests** provide a way to design your infrastructures by creating classes for each of them.

```csharp
using NotoriousTest.Common.Infrastructures.Sync;

public class SQLServerDBInfrastructure : Infrastructure
{
    public SQLServerDBAsyncInfrastructure(bool initialize = false) : base(initialize){}
    public override void Initialize()
    {
        // Here you can create the database
    }

    public override void Reset()
    {
        // Here you can empty the database
    }
    public override void Destroy()
    {
        // Here you can destroy the database
    }
}
```

Async version :

```csharp
using NotoriousTest.Common.Infrastructures.Async;

// Async version
public class SQLServerDBAsyncInfrastructure : AsyncInfrastructure
{
    public SQLServerDBAsyncInfrastructure(bool initialize = false) : base(initialize){}

    public override Task Initialize()
    {
        // Here you can create the database
    }

    public override Task Reset()
    {
        // Here you can empty the database
    }
    public override Task Destroy()
    {
        // Here you can destroy the database
    }
}
```

As you can see, an infrastructure is made of 3 main lifecycle method.

- **`Initialize`** : You can call this method **to start an infrastructure** or set the initialize boolean in the constructor at true.

- **`Reset`** : you can call this method **to reset an infrastructure**. Reset mean to put the infrastructure in an empty state without recreating it from scratch (essentialy for perfomances).

- **`Destroy`** : you can call this method **to destroy an infrastructure**, **`Destroy`** mean to completly delete the infrastructure.

Once you made it, you can use this infrastructure as standalone in your tests :

```csharp
[Fact]
public async Task Database_Creation()
{
    // Dont forget to put initialize at true, so you dont have to call **Initialize**
    await using (var db = new SQLServerDBAsyncInfrastructure(initialize: true))
    {
        // Test logic here...
    }
    // Disposing an infrastructure will automatically call Destroy()
}
```

Even if using an infrastructures in standalone can be easy, it should be a best practices to use them inside an `Environment`. Let's see why and how.

### Environment

We have seen how we could use an infrastructure directly within a tests, but it will become really difficult to maintain if we do this for every test and for every infrastructure.

That's why we provide this environment feature :

An environment is a collection of infrastructure drived by tests' lifecycle.

![Cycle de vie des infrastructures](./Documentation/Images/Infrastructure%20lifecycle.png)

Let's create an environment :

```csharp
public class SampleEnvironment : Environment
{
    public override void ConfigureEnvironment()
    {
        // Add all your infrastructure here.
        AddInfrastructure(new DatabaseInfrastructure());
    }
}
```

Async version :

```csharp
public class SampleEnvironment : AsyncEnvironment
{
    public override Task ConfigureEnvironmentAsync()
    {
        // Add all your infrastructure here.
        AddInfrastructure(new DatabaseInfrastructure());

        return Task.CompletedTask;
    }
}
```

Then, make your test class inherit from `IntegrationTest`.

```csharp
public class UnitTest1 : IntegrationTest<SampleEnvironment>
{
    public UnitTest1(SampleEnvironment environment) : base(environment)
    {
    }

    [Fact]
    public async void MyTest()
    {
        // Access infrastructure by calling
        SQLServerDBAsyncInfrastructure infrastructure = await CurrentEnvironment.GetInfrastructure<SQLServerDBAsyncInfrastructure>();
    }
}
```

Async version:

```csharp
public class UnitTest1 : AsyncIntegrationTest<SampleEnvironment>
{
    public UnitTest1(SampleEnvironment environment) : base(environment)
    {
    }

    [Fact]
    public async Task MyTest()
    {
        // Access infrastructure by calling
        SQLServerDBAsyncInfrastructure infrastructure = await CurrentEnvironment.GetInfrastructureAsync<SQLServerDBAsyncInfrastructure>();
    }
}
```

## Advanced functionalities

### Ordering infrastructures execution

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

### Advanced Control Over Infrastructure Resets

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

### Web

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

### TestContainers

**NotoriousTest.TestContainers** is now available as a separate package.

Install [NotoriousTest](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest.TestContainers
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest.TestContainers
```

This package provides classes that automatically start and stop the container at the beginning and end of the test campaign!
It introduces three new classes:

- `DockerContainerAsyncInfrastructure<TContainer>` : Standard infrastructure.
- `ConfiguredDockerContainerAsyncInfrastructure<TContainer>`: Infrastructure handling configuration as a dictionary.
- `ConfiguredDockerContainerAsyncInfrastructure<TContainer, TConfiguration>` : Infrastructure handling a configuration object.

> ❗ Since `TestContainers` doesn't support synchronous code, theses classes are only available in an `AsyncEnvironment`.

Here's an example :

```csharp

public class SqlServerContainerInfrastructure : DockerContainerAsyncInfrastructure<GenericContainer>
{
    public override Container {get; init;} = new MsSqlBuild().Build();

    public SampleDockerContainer(bool initialize = false) : base(initialize)
    {
    }

    public override Task Reset()
    {
        return Task.CompletedTask;
    }
}
```

### Sql Server

#### Initialization

**NotoriousTest.SqlServer** is now available as a separate package.

Install [NotoriousTest.SqlServer](https://www.nuget.org/packages/NotoriousTest.SqlServer/) from the package manager console:

```
PM> Install-Package NotoriousTest.SqlServer
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest.SqlServer
```

You can now simply use the SqlServerContainerAsyncInfrastructure to start a SQL Server database.

It will automatically start at the beginning of the test campaign, stop at the end, and reset between each test, powered by TestContainers and Respawn.

Here's an example:

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }
    }
```

This infrastructure performs several tasks:

- **On initialization:**
  - Starts a SQL Server Docker container, powered by TestContainers.
  - Creates a unique database.
- **On reset:**
  - Empties the database, powered by Respawn.
- **On destruction:**
  - Stops the container.

#### Test Usage

The infrastructure provides two methods:

- **`GetDatabaseConnection`** – Returns a `SqlConnection` pointing to the newly created database for your test.
- **`GetDatabaseConnectionString`** – Returns a connection string pointing to the newly created database for your test.

```csharp
[Fact]
public async Task Test1()
{
    SqlServerInfrastructure sqlInfrastructure = await CurrentEnvironment.GetInfrastructureAsync<SqlServerInfrastructure>();
    await using(SqlConnection sql = sqlInfrastructure.GetDatabaseConnection())
    {
        // Arrange your database here.
    }
}
```

#### Populating the database

You can populate the database by overriding the `PopulateDatabase` method :

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        protected override async Task PopulateDatabase(SqlConnection connection)
        {
            // Play all your migrations script here, use DBUp or any other migration tool
            await CreateTables(connection);
        }
    }
```

#### Generating configuration

You can generate configuration for your web application by overriding the `Initialize` method :

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            // We can add the connection string to the configuration.
            Configuration.Add("ConnectionStrings:SqlServer", GetDatabaseConnectionString());
        }
    }
```

#### Configure the container

You can configure the container by overriding the `ConfigureSqlContainer` method :

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        protected override MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            // Configure the builder to override image, host, password, port, etc.
            return builder.WithPassword("NotoriousStrong(!)Password6");
        }
    }
```

#### Configure respawn settings

To configure respawn settings, you can assign RespawnOptions property. They will be used by respawn to reset the database.

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
            RespawnOptions = new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "MyMigrationTable" }
            };
        }
    }
```

#### Configure the database name

By default, Database name will be `NotoriousDb` concatened with the `ContextId`.
You can override `NotoriousDb` by your custom name by setting the property `DbName`.

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
            DbName = "TestDb";
        }
    }
```

## Hands-On Examples

Get started quickly with practical examples available in the [Samples](./Samples/NotoriousTests.InfrastructuresSamples/) folder. These examples demonstrate how to set up and use NotoriousTests for real-world scenarios.

### What's included:

- **[SqlServerInfrastructures.cs](./Samples/NotoriousTests.InfrastructuresSamples/Infrastructures/SqlServerInfrastructures.cs)**  
  Learn how to manage your SQL Server database using Respawn, TestContainers and plain SQL for creating, destroying, and resetting your database seamlessly.
- **[TestWebApplication.cs](./Samples/NotoriousTests.InfrastructuresSamples/Infrastructures/TestWebApplication.cs)**  
  See how to configure a WebApplicationFactory with in-memory configuration for fast and isolated tests.
- **[TestEnvironment](./Samples/NotoriousTests.InfrastructuresSamples/Environments/TestEnvironment.cs)**  
  Understand how to set up environments to manage multiple infrastructures effortlessly.
- **[SampleTests.cs](./Samples/NotoriousTests.InfrastructuresSamples/SampleTests.cs)**  
  Dive into this file to see how to access infrastructures and use them in your tests.
- **[Program.cs](./Samples/NotoriousTests.InfrastructuresSamples.TestWebApp/Program.cs)**  
  Explore how the SqlServerInfrastructure generates configuration for the Web Application.

## Other nugets i'm working on

- [**NotoriousClient**](https://www.nuget.org/packages/NotoriousClient/) : Notorious Client is meant to simplify the sending of HTTP requests through a fluent builder and an infinitely extensible client system.
- [**NotoriousModules**](https://github.com/Notorious-Coding/Notorious-Modules) : Notorious Modules provide a simple way to separate monolith into standalone modules.
