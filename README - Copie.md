## ![Logo](./Documentation/Images/NotoriousTest.png)

**Notorious Test** provide a simple way to isolate integration tests. Based en XUnit.

## Summary

- [Support](#support)
- [Features](#features)
- [Motivation](#motivation)
- [Getting started](#getting-started)

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

## Getting Started

### Setup

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [NotoriousClient](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest
```

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

This is not the main use case of using infrastructure, later, we will see how to setup a whole environment with multiple infrastructure usable for you test.
But first, let's see all other infrastructures available.

#### Configurable infrastructures

Sometime, you will need to **produce configuration data** (essentialy connection strings but whatever you want). There an infrastructure for that, lets modify our **SQLServerDBInfrastructure** :

```csharp
public class SqlServerConfiguration{
    public string ConnectionString { get; set; }
}

public class SQLServerDBInfrastructure : ConfigurableInfrastructure<SqlServerConfiguration>
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
public class SQLServerDBAsyncInfrastructure : ConfigurableAsyncInfrastructure<SqlServerConfiguration>
{
    public override int Order => 1;

    public SQLServerDBAsyncInfrastructure(bool initialize = false) : base(initialize){}

    public override Task Initialize()
    {
        // Here you can create the database
        // Assign configuration
        // You could even create a DB Connection and store it in a public readonly variable.
        Configuration.ConnectionString = "MySweetConnectionString";
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

If you dont want to provide a type for your configuration, **ConfigurableAsyncInfrastructure** or **ConfigurableInfrastructure** can be use without generic type. Your configuration will be a simple **Dictionary<string, string>**.

You can get complex configuration as **Dictionnary<string, string>** by calling **GetConfigurationAsDictionary()** on the infrastructure. This one will serialize your configuration as a dictionary with key being the path to the value.

Such as :

- Key : Path.To.ConnectionString
- Value : MyConnectionString

This will be useful to use for overriding json configuration.

#### Web Application Infrastructures

Microsoft provide a simple way to create fake server so your integration tests can call your API. It is called **WebApplicationFactory**.

See the docs for WebApplicationFactory usage: [Integration tests in ASP.NET Core
](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-7.0)

The simplest way to create a web application infrastructure is to use **WebApplicationInfrastructure** :

```csharp
using NotoriousTest.Web.Infrastructures;

internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program>
{
    public SampleProjectWebApplicationInfrastructure() : base()
    {
    }
}
```

Async version :

```csharp
internal class SampleProjectWebApplicationInfrastructure : WebApplicationAsyncInfrastructure<Program>
{
    public SampleProjectWebApplicationInfrastructure() : base()
    {
    }
}

```

It will **automaticaly create a WebApplicationFactory for you**, and run it at initialize.

**Downside of this is that you can't override anything from your app.** It could be a bit inconvenient when testing, cause we could want to mock some calls (even in integration testing).

So lets create a custom app, so we can override everything.

To create an application, we will inherits from this class.

```csharp
using Microsoft.AspNetCore.Mvc.Testing;

public class SampleProjectApp : WebApplicationFactory<Program>
{
    // You could override whatever you want here. Microsoft provide various way to customise WebApplication
}
```

Program is the underlying class used in your Program.cs from your ASP.NET project.
To use it, we will need it as public.

```csharp
    public class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            ...
            // Program code
        }
    }
```

Now we can use this application is our **SampleProjectWebApplicationInfrastructure**

```csharp
using NotoriousTest.Web.Infrastructures;

internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program>
{
    public SampleProjectWebApplicationInfrastructure() : base(new SampleProjetApp())
    {
    }
}
```

Async version :

```csharp
internal class SampleProjectWebApplicationInfrastructure : WebApplicationAsyncInfrastructure<Program>
{
    public SampleProjectWebApplicationInfrastructure() : base(new SampleProjectApp())
    {
    }
}
```

Now you can use this infrastructure in your tests, as well as other infrastructures.

Before this, we have talked about configuration. Web application configuration are often override by inmemory collection represented by a **Dictionary<string, string>**.

That's why we provide a simple way to override configuration of a WebApplication.

```csharp
using NotoriousTest.Web;

public class SampleProjectApp : ConfiguredWebApplication<Program>
{
}
```

Then, you could just call the **"Configure" method of "SampleProjectApp"** and pass your configuration.

Here is one of many ways to do that within an infrastructure :

```csharp
internal class SampleProjectWebApplicationInfrastructure : WebApplicationAsyncInfrastructure<Program>
{
    public SampleProjectWebApplicationInfrastructure(Dictionary<string, string> configuration)
        : base(new SampleProjectApp().Configure(configuration))
    {
    }
}
```

We have seen how we could use an infrastructure directly within a tests, but it will become really difficult to maintain if we do this for every test and for every infrastructure.

That's why we provide environment feature :

### Environment

An environment is a collection of infrastructure drived by tests's lifecycle.

![Cycle de vie des infrastructures](./Documentation/Images/Infrastructure%20lifecycle.png)

Let's create an environment :

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

Async version :

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

Then, inherit your test class with **Integration tests**.

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

#### Web Environment

Before, we have setup a WebApplicationInfrastructure and a ConfiguredWebApp

Let's use them inside our Environment :

```csharp
public class SampleEnvironment : AsyncWebEnvironment<Program>
{
    public override Task ConfigureEnvironmentAsync()
    {
        AddInfrastructure(new DatabaseInfrastructure());

        // Add your configured web application
        AddWebApplication(new SampleProjectApp());

        return Task.CompletedTask;
    }
}
```

> :information: **AsyncWebEnvironment** will automatically get infrastructures configuration, and add theses configurations to the web app, as an InMemoryCollection.

Now that our infrastructure is setup, we can create an integration test.

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
        WebApplicationAsyncInfrastructure<TEntryPoint> WebAppInfrastructure = await CurrentEnvironment.GetWebApplication();

        // Get the HTTP Client
        HttpClient client = infrastructure.HttpClient;

        // Call your API
        HttpResponseMessage response = await client.GetAsync("api/weather");

        // Check whatever you want
        Assert.True(response.IsSuccessStatusCode);
    }
}
```

Nice ! Good job, now, your integration test are isolated from each other.
