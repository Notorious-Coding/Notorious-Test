## ![Logo](./Documentation/Images/NotoriousTest.png)

**Notorious Test** provide a simple way to isolate integration tests. Based en XUnit.

## Summary

- [Support](#support)
- [Features](#features)
- [Motivation](#motivation)
- [Getting started](#getting-started)
- [Test scoped infrastructures](#test-scoped-infrastructures)

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

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [NotoriousClient](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest
```

First you will need to define a list of necessary infrastructures for your tests to work, such a Databases, API, FTP, etc...

Then create these infrastructures in the code by implementing **Infrastructure** base class, for example lets create an infrastructure database :

```csharp
public class DatabaseInfrastructure : Infrastructure
{
    // Define the order of execution
    public override int Order => 1;

    public override void Destroy()
    {
        // Type your code to destroy the database
    }

    public override void Initialize()
    {
        // Type your code to create your database
    }

    public override void Reset()
    {
        // Type your code to reset all the data in the database
    }
}
```

You will definitely need to start your web application to call your endpoints.

First of all, lets create a WebApplicationFactory (like you would do for basic integration tests) :

```csharp
internal class SampleProjectApp : WebApplicationFactory<Program>
{
    // Override what you need to override
}
```

Dont forget to turn on **InternalsVisibleTo** to use the Program entry point class.

```xml
<ItemGroup>
    <InternalsVisibleTo Include="Your.Test.Project" />
</ItemGroup>
```

In addition, reference the web project in your test project so the WebApplicationFactory have access to it.

Then, an infrastructure for a WebApp is ready for you, just inherit from **WebApplicationInfrastructure** :

```csharp
internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program>
{
    public SampleProjectWebApplicationInfrastructure(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory)
    {
    }
}
```

Once you have built all necessaries infrastructures, you can now configure an Environment, for this, inherit from **Environment** base class, and override the **_ConfigureEnvironment_** method:

```csharp
public class SampleEnvironment : Environment
{
    public override void ConfigureEnvironment(EnvironmentConfig config)
    {
        config
            .AddInfrastructures(new SampleProjectWebApplicationInfrastructure(new SampleProjectApp()))
            .AddInfrastructures(new DatabaseInfrastructure());
    }
}
```

The last thing you will need to do is to inherit from **IntegrationTest** in all of your tests class:

```csharp
public class UnitTest1 : IntegrationTest<SampleEnvironment>
{
    public UnitTest1(SampleEnvironment environment) : base(environment)
    {
    }
}
```

Contragulations ! Now you are ready to test your applications serenely.

Before every tests, it will reset all your infrastructures, before every test campaign, it will create all your infrastructures, and after every test campaign, it will destroy every infrastructures !

Here's how u can use your infrastructure in your tests :

```csharp
[Fact]
public async Task Test2()
{
    HttpClient client = CurrentEnvironment.GetInfrastructure<SampleProjectWebApplicationInfrastructure>().HttpClient;

    HttpResponseMessage response = await client.GetAsync("api/weather");
    Assert.True(response.IsSuccessStatusCode);

    string content = await response.Content.ReadAsStringAsync();
}
```

## Test scoped infrastructures?

You can use infrastructures as standalone :

```csharp
public class DatabaseInfrastructure : Infrastructure
{
    // Best practice is to declare a constructor in your infrastructure.
    public DatabaseInfrastructure(bool initialize = false) : base(initialize)
    {
    }

    // ...rest
}
```

```csharp
[Fact]
public void Test2()
{
    // Infrastructure will be created if initiliaze is true.
    using(var db = new DatabaseInfrastructure(initialize: true))
    {
        HttpClient client = CurrentEnvironment.GetInfrastructure<SampleProjectWebApplicationInfrastructure>().HttpClient;

        HttpResponseMessage response = await client.GetAsync("api/weather");
        Assert.True(response.IsSuccessStatusCode);

        string content = await response.Content.ReadAsStringAsync();
    } // then, at the end of the using, it will be destroyed.
}

[Fact]
public void Test2()
{
    using(var db = new DatabaseInfrastructure())
    {
        // You will need to initialize infrastructures if you dont pass initiliaze = true;
        db.Initialize();

        HttpClient client = CurrentEnvironment.GetInfrastructure<SampleProjectWebApplicationInfrastructure>().HttpClient;

        HttpResponseMessage response = await client.GetAsync("api/weather");
        Assert.True(response.IsSuccessStatusCode);

        string content = await response.Content.ReadAsStringAsync();
    } // then, at the end of the using, it will be destroyed.
}

```

:warning: Note : Every classes used here have a **Async Implementation** for **Asynchronous Testing**.

Use **AsyncInfrastructure** for Asynchronous Infrastructures.\
Use **AsyncEnvironment** for Asynchronous Environment.\
Use **AsyncIntegrationTest<>** for Asynchronous Integration Test.
