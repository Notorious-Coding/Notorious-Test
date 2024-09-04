## ![Logo](./Documentation/Images/NotoriousTest.png)

**Notorious Test** provide a simple way to isolate integration tests. Based en XUnit.

## Summary

- [Support](#support)
- [Features](#features)
- [Motivation](#motivation)
- [Getting started](#getting-started)
- [Setup](#setup)
- [Base functionalities](#base-functionalities)
  - [Infrastructures](#infrastructures)
  - [Environment](#environment)
- [Advanced functionalities](#advanced-functionalities)
  - [Configuration](#configuration)
    - [Configurable Infrastructures](#configurable-infrastructures)
    - [Configurable Environment](#configurable-environment)
  - [Web](#web)
    - [Web Application Infrastructure](#web-application-infrastructure)
    - [Web Environment](#web-environment)

## Support

- Net6/7

## Features

- Easy share of test infrastructure
- Easy building of test framework.
- Complete isolation of integration tests.
- Simple implementation.
- Based on XUnit.

## Motivation

The goal is to provide a way to make integration test without worrying about data collision, side effects, and infrastructure sharing.

## Setup

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [NotoriousClient](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest
```

## Base functionnalities

### Infrastructures

An infrastructure is a piece of hardware or software that is necessary for your app to work.
For example, a SQL Server Database is an infrastructure.
**NotoriousTests** provide a way to design your infrastructures by creating classes for each of them.

```csharp
using NotoriousTest.Common.Infrastructures.Sync;

public class SQLServerDBInfrastructure : Infrastructure
{
    public override int Order => 1;

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
    public override int Order => 1;

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

**To start an infrastructure**, you can call **Initialize** or put the initialize boolean in the constructor at true.

**To reset an infrastructure**, you can call **Reset**. Reset mean to put the infrastructure in an empty state without recreating it from scratch (essentialy for perfomances).

**To destroy an infrastructure**, you can call **Destroy** to completly delete the infrastructure.

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

This is not the main use of infrastructures, now, we will see how to setup a whole environment.

### Environment

We have seen how we could use an infrastructure directly within a tests, but it will become really difficult to maintain if we do this for every test and for every infrastructure.

That's why we provide environment feature :

An environment is a collection of infrastructure drived by tests's lifecycle.

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

Then, inherit your test class with **IntegrationTest**.

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

### Configuration

In some cases, you will need to handle configuration from your infrastructures, such as connection string, or secrets, etc. **Notorious Tests** provide a nice way to produce and consume configuration within infrastructures.

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
            Configuration.Add("DatabaseConfiguration.ConnectionString", "Test");
        }

        public override void Reset(){}
    }
```

Now, you can access directly your infrastructure configuration within a test :

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

You could use **ConfiguredInfrastructure** within **Environment**.

First, inherit from **ConfiguredEnvironment** instead of **Environment**.
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

Within environment, you would use the public property **EnvironmentConfiguration** to access produced configuration from infrastructures.

> :warning: Inside an environment, **configuration object should represent the whole app configuration**. It will move from infrastructures to infrastructures, and every infrastructures will edit his own part of the configuration.

One more step is needed so your environment understand how your infrastructures handle configuration.

If your infrastructure will **consume the environment configuration**, then, inherit from **IConfigurationConsumer**

If your infrastructure will **produce environment configuration**, then, inherit from **IConfigurationProducer**.

If it does both, then inherits from both.
Here's an example :

```csharp
public class DatabaseInfrastructure : ConfiguredInfrastructure<Configuration>, IConfigurationProducer
{

    public DatabaseInfrastructure(bool initialize = false) : base(initialize)
    {
    }

    public override int Order => 1;

    public override void Destroy(){}

    public override void Initialize()
    {
        // We produce a connection string, so we use IConfigurationProducer.
        Configuration.DatabaseConfiguration = new DatabaseConfiguration()
        {
            ConnectionString = "Test"
        };
    }

    public override void Reset(){}
}
```

### Web

Within web applications, you will certainly need to add an WebApplication that work in background so your tests can call an actual API.

**NotoriousTests** provide everything so you donc need to scratch your head too much.

#### Web Application Infrastructure

First, you will need to create an **WebApplication**.

```csharp
internal class SampleProjectApp : WebApplication<Program>{}
```

This is actually a **WebApplicationFactory** provided by .NET ([See microsoft doc for more information.](https://learn.microsoft.com/fr-fr/aspnet/core/test/integration-tests?view=aspnetcore-8.0))

Here you can override everything you need for your app to be fully functional.

Then, let's create an **WebApplicationInfrastructure** and pass our **WebApplication**.

> :information: **WebApplicationInfrastructure** are necessarily a **IConfigurationConsumer**. Configuration passed will be added as configuration on your web app, making it accessible within **IConfiguration** object in _Program.cs_.

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

Your web application will start automatically at the start of a test campaign.

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
