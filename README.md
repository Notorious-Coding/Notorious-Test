## ![Logo](./Documentation/Images/NotoriousTest.png)


__Clean, isolated, and maintainable integration testing for .NET__

[![NuGet](https://img.shields.io/nuget/v/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![License](https://img.shields.io/github/license/Notorious-Coding/Notorious-Test)](https://github.com/Notorious-Coding/Notorious-Test/blob/master/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-8%2B-blue)](https://dotnet.microsoft.com/)
[![Build Status](https://github.com/Notorious-Coding/Notorious-Test/actions/workflows/release.yml/badge.svg)](https://github.com/Notorious-Coding/Notorious-Test/actions/workflows/release.yml)
[![GitHub stars](https://img.shields.io/github/stars/Notorious-Coding/Notorious-Test?style=social)](https://github.com/Notorious-Coding/Notorious-Test/stargazers)

If you plan to use this NuGet package, let me know in the [Tell me if you use that package !](https://github.com/Notorious-Coding/Notorious-Test/discussions/1) discussion on Github ! Gaining insight into its usage is very important to me!

## Summary
- [Purpose](#purpose)
- [Hello World](#hello-word)
- [Resources & Community](#resources--community)
  - [Documentation](#documentation)
  - [Changelog](#changelog)
  - [Contact](#contact)
- [Other packages i'm working on](#other-nugets-im-working-on)

## Purpose

Have you ever had to write and rewrite boilerplate code to set up a database, reset data between each test, or tear down containers?
All that setup required to keep your integration tests fully isolated, and ensure their maintainability, reproducibility, and efficiency.

**NotoriousTests** removes the need to build all of that yourself.

The concept is simple:

1. Create an **infrastructure** and implement its _initialization_, _reset_, and _destruction_ logic.
2. Add it to an **environment**.
3. Access it directly from your **integration tests**.

**NotoriousTests** will automatically manage the **lifecycle of your infrastructures.**

## Hello World

## Setup

First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [NotoriousTest](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest
```

## Define a Basic Infrastructure

An **infrastructure** represents an **external dependency** (database, message bus, etc.).

For now, we’ll define an empty infrastructure to illustrate the setup.
You can replace `MyInfrastructure` with any real infrastructure later.

```csharp
public class MyInfrastructure : Infrastructure
{
    public MyInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    public override Task Initialize()
    {
        // Setup logic here (e.g., start a database, configure an API)
        Console.WriteLine($"Setup of {nameof(MyInfrastructure)}");
        return Task.CompletedTask;
    }

    public override Task Reset()
    {
        // Reset logic here (e.g., clear data, reset state)
        Console.WriteLine($"Reset of {nameof(MyInfrastructure)}");
        return Task.CompletedTask;
    }

    public override Task Destroy()
    {
        // Cleanup logic here (e.g., shut down services)
        Console.WriteLine($"Shutdown of {nameof(MyInfrastructure)}");
        return Task.CompletedTask;
    }
}
```

**📌 What this does:**

- Defines a basic infrastructure with lifecycle methods.
- This is where you would add setup logic for databases, APIs, queues, etc.

## Create a Web Application

Now, let's create a basic web application for our tests.

```csharp
public class SampleWebApp : WebApplication<Program>
{
    // Override WebApplicationFactory methods.
}
```

> ❗ This is a **`WebApplicationFactory`** customized for `NotoriousTest`.

📌 **What this does:**

- Defines a minimal web application factory for testing.

## Create a Test Environment

A test environment groups infrastructures together.

```csharp
public class MyTestEnvironment : NotoriousTest.XUnit.Environment
{
    public MyTestEnvironment(IMessageSink sink) : base(sink) { }
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<MyInfrastructure>(); // Register via DI
        this.AddWebApplication<SampleWebApp>(); // Register the web app
    }
}
```

📌 **What this does:**

- Registers MyInfrastructure and SampleWebApp inside the test environment.
- Ensures all tests run in a clean and isolated setup.

## Write your first test

Now, let's write a basic integration test using our environment.

```csharp
public class MyIntegrationTests : IntegrationTest<MyTestEnvironment>
{
    public MyIntegrationTests(MyTestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task ExampleTest()
    {
        // Retrieve the infrastructure
        var infra = CurrentEnvironment.GetInfrastructure<MyInfrastructure>();

        // Add test logic here (e.g., verify database state, call an API)

        Assert.NotNull(infra); // Basic validation to confirm setup works
    }
}
```

📌 **What this does:**

- Retrieves the test infrastructure from the environment.
- You can add real test logic (API calls, database assertions, etc.).

## 🚀 Running Your First Test

Now, let's run the test:

```sh
dotnet test
```

### Expected Output

You should see something like:

```sh
Passed! 1 test successful.
```

If everything works, congrats! 🎉 You’ve successfully set up NotoriousTest.

## Resources & Community

### Documentation

- 📖 [Core Concepts](./Documentation/2-core-concepts.md) – Learn how infrastructures and environments work.
- ⚡ [Advanced Features](./Documentation/3-advanced-features.md) – Discover ordering, reset behaviors, and more. \
- 🔌 [Integrations](./Documentation/4-integrations.md) – See how to integrate SQL Server, TestContainers, and more.
- 📚 [Examples](./Documentation/5-example.md) – Hands-on use cases with real-world setups.

### Changelog

You can find the changelog [here](./CHANGELOG.md).

### Contact

Have questions, ideas, or feedback about NotoriousTests?
Feel free to reach out! I'd love to hear from you. Here's how you can get in touch:

- GitHub Issues: [Open an issue](https://github.com/Notorious-Coding/Notorious-Test/issues) to report a problem, request a feature, or share an idea.
- Email: [briceschumacher21@gmail.com](mailto:briceschumacher21@gmail.com)
- LinkedIn : [Brice SCHUMACHER](http://www.linkedin.com/in/brice-schumacher)

The discussions tabs is now opened !
Feel free to tell me if you use the package here : https://github.com/Notorious-Coding/Notorious-Test/discussions/1 !

## Other nugets i'm working on

- [**NotoriousClient**](https://www.nuget.org/packages/NotoriousClient/) : Notorious Client is meant to simplify the sending of HTTP requests through a fluent builder and an infinitely extensible client system.
- [**NotoriousModules**](https://github.com/Notorious-Coding/Notorious-Modules) : Notorious Modules provide a simple way to separate monolith into standalone modules.
