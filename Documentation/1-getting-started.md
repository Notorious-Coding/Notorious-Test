# Getting started

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
public class MyInfrastructure : AsyncConfiguredInfrastructure
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

> ❗ This is a `WebApplicationFactory` customized for `NotoriousTest`.

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

## ✅ Next Steps

Now that your first test is running, explore more features:

- 📖 [Core Concepts](./2-core-concepts.md) – Learn how infrastructures and environments work.
- ⚡ [Advanced Features](./3-advanced-features.md) – Discover ordering, reset behaviors, and more. \
- 🔌 [Supported Infrastructures](./4-integrations.md) – See how to integrate SQL Server, TestContainers, and more.

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
