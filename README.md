## ![Logo](./Documentation/Images/NotoriousTest.png)

[![NuGet](https://img.shields.io/nuget/v/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![License](https://img.shields.io/github/license/Notorious-Coding/Notorious-Test)](https://github.com/Notorious-Coding/Notorious-Test/blob/master/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-6%2B-blue)](https://dotnet.microsoft.com/)
[![Build Status](https://github.com/Notorious-Coding/Notorious-Test/actions/workflows/release.yml/badge.svg)](https://github.com/Notorious-Coding/Notorious-Test/actions/workflows/release.yml)
[![GitHub stars](https://img.shields.io/github/stars/Notorious-Coding/Notorious-Test?style=social)](https://github.com/Notorious-Coding/Notorious-Test/stargazers)

Clean, isolated, and maintainable integration testing for .NET

If you plan to use this NuGet package, let me know in the [Tell me if you use that package !](https://github.com/Notorious-Coding/Notorious-Test/discussions/1) discussion on Github ! Gaining insight into its usage is very important to me!

## Summary

- [The problem with integration testing in .NET](#-the-problem-with-integration-testing-in-net)
- [NotoriousTest: The solution](#-notorioustest-the-solution)
- [Why use NotoriousTest](#why-use-notorious-test)
- [Hello World](#hello-word)
- [Resources & Community](#resources--community)
  - [Documentation](#documentation)
  - [Changelog](#changelog)
  - [Contact](#contact)
- [Other packages i'm working on](#other-nugets-im-working-on)

## 🚨 The problem with integration testing in .NET

When testing an application that relies on multiple infrastructures (databases, message buses, blob storage, FTP, SMTP...), the common approach looks like this:

- Create a **WebApplicationFactory** and use it as a fixture.
- Implement `IAsyncLifetime` to initialize and dispose of infrastructures.
- Manage each infrastructure (SQL Server, RabbitMQ, Redis, MongoDB, etc.) within `Initialize` and Dispose.
- Add specific setup code inside the test constructors.

👉 This works for small projects, but as complexity increases, it becomes a nightmare:

🔥 The **`WebApplicationFactory`** turns into an unmanageable beast.\
🔄 Everything needs to be manually refactored and structured → resulting in messy, hard-to-maintain code. \
🏗️ Tests become slow to write and complex to maintain. \
⏳ Setup time skyrockets, reducing productivity.

And that’s the real problem—**integration tests are meant to ensure the quality of our code**. But if they themselves become unmanageable, **the enemy of our enemy becomes our enemy**.

## 🚀 **NotoriousTest: The solution**

NotoriousTest introduces a modular and structured approach to isolating and managing your test infrastructures effortlessly.

✅ Each infrastructure (database, message bus, etc.) is encapsulated in its own class with a proper lifecycle (Init, Reset, Destroy). \
✅ Test environments are composable → you assemble infrastructures like Lego blocks. \
✅ Automatic reset between tests ensures proper isolation. \
✅ No need to bloat your WebApplicationFactory → each infrastructure is cleanly defined. \
✅ Works seamlessly with TestContainers, SQL Server, and more.

👉 The result? A testing framework that is clean, modular, maintainable, and scalable.

## Why use NotoriousTest

🔹 **Designed for complex integration tests** \
Seamlessly manage multiple infrastructures like databases, message buses, blob storage, and more, without turning your test setup into a nightmare.

**🔹 Fully isolated and resettable environments** \
Ensure clean, independent tests by automatically resetting infrastructures between each run—no leftover data, no hidden side effects.

**🔹 Modular & reusable infrastructure components** \
Define each infrastructure separately and compose test environments like Lego blocks. No more bloated WebApplicationFactories!

**🔹 Effortless setup & minimal boilerplate** \
Forget complex test initialization logic—NotoriousTest takes care of instantiating, resetting, and destroying your infrastructures automatically.

**🔹 Compatible with TestContainers & SQL Server** \
Seamlessly integrates with Dockerized test environments, allowing you to spin up databases, message queues, and more in seconds.

**🔹 Powered by XUnit & Async Support** \
Leverage the flexibility of XUnit’s dependency injection and fully async infrastructure lifecycles for faster and more scalable tests.

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
public class MyInfrastructure : AsyncInfrastructure
{
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
public class MyTestEnvironment : AsyncWebEnvironment
{
    public override async Task ConfigureEnvironmentAsync()
    {
        AddInfrastructure(new MyInfrastructure()); // Register the test infrastructure
        AddWebApplication(new SampleWebApp()); // Register the web app
    }
}
```

📌 **What this does:**

- Registers MyInfrastructure and SampleWebApp inside the test environment.
- Ensures all tests run in a clean and isolated setup.

## Write your first test

Now, let's write a basic integration test using our environment.

```csharp
public class MyIntegrationTests : AsyncIntegrationTest<MyTestEnvironment>
{
    public MyIntegrationTests(MyTestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task ExampleTest()
    {
        // Retrieve the infrastructure
        var infra = await CurrentEnvironment.GetInfrastructureAsync<MyInfrastructure>();

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
