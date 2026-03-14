# 🏗️ Core Concepts

NotoriousTest provides a structured way to manage **test infrastructures** and ensure **clean, isolated integration tests**.  
This document introduces the core concepts and how they work together.

## 🏗️ What is an Infrastructure?

An **infrastructure** represents an external dependency required for testing.  
This could be a **database, message queue, file storage, API client**, or any service that needs to be set up and managed during tests.

### ✨ **Example of an Infrastructure**

```csharp
public class MyInfrastructure : AsyncInfrastructure
{
    public override Task Initialize()
    {
        // Setup logic here (e.g., start a database, configure an API)
        return Task.CompletedTask;
    }

    public override Task Reset()
    {
        // Reset logic here (e.g., clear data, reset state)
        return Task.CompletedTask;
    }

    public override Task Destroy()
    {
        // Cleanup logic here (e.g., shut down services)
        return Task.CompletedTask;
    }
}
```

📌 Key Points:

- `Initialize()` → Called once at the start of the test session.
- `Reset()` → Called before each test to ensure data isolation.
- `Destroy()` → Called at the end of the test session to clean up resources

## 🌍 What is a Test Environment?

A test environment is a collection of infrastructures that define the full setup for running tests.
Instead of manually initializing infrastructures in every test, you define them once inside an environment.

✨ **Example of a Test Environment**

```csharp
public class MyTestEnvironment : AsyncEnvironment
{
    public override async Task ConfigureEnvironmentAsync()
    {
        // Register infrastructures
        AddInfrastructure(new MyInfrastructure());
        AddInfrastructure(new MyInfrastructure2());
    }
}
```

📌 **Key Points:**

- Environments encapsulate multiple infrastructures.
- They ensure consistency across all tests.
- They handle initialization, reset, and cleanup automatically.

## 🔄 How Does the Test Lifecycle Work?

NotoriousTest manages your test lifecycle automatically to ensure clean and isolated tests.

**Test Session Starts** \
Infrastructures are initialized (Initialize()).

**Before Each Test** \
Infrastructures are reset (Reset()).

**Test Runs** \
The test executes in an isolated environment.

**After Test Suite Completes** \
Infrastructures are destroyed (Destroy()).

🔥 **Why is this important?** \
✅ Guarantees clean data for each test (no unwanted side effects). \
✅ Prevents state leaks between tests. \
✅ Eliminates manual setup/teardown code in test classes.

## 🔌 How to Access Infrastructures in Tests

Once a test environment is configured, you can retrieve any registered infrastructure inside your tests.

**✨ Example Test with an Infrastructure**

```csharp

public class MyIntegrationTests : IntegrationTest<MyTestEnvironment>
{
    public MyIntegrationTests(MyTestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task ExampleTest()
    {
        // Retrieve the infrastructure from the environment
        var infra = await CurrentEnvironment.GetInfrastructureAsync<MyInfrastructure>();

        // Use the infrastructure
        Assert.NotNull(infra); // Example check
    }
}
```

📌 **Key Takeaways:**

- GetInfrastructureAsync<T>() → Retrieves an infrastructure inside a test.
- No need for manual setup inside test classes.
- Tests remain clean and focused on logic instead of infrastructure handling.

✅ Summary
| Concept | Description |
| -----------------|---------------------------------------------------------|
| Infrastructure | Represents an external dependency (DB, API, Queue). |
| Test Environment | Groups infrastructures together for consistent tests.|
| Lifecycle | Ensures clean initialization, reset before tests, and proper cleanup.|
| Retrieving Infra | Use GetInfrastructureAsync<T>() inside tests. |

Now that you understand the fundamentals, check out:

- ⚡ [Advanced Features](./3-advanced-features.md) – Discover ordering, reset behaviors, and more. \
- 🔌 [Supported Infrastructures](./4-integrations.md) – See how to integrate SQL Server, TestContainers, and more.
- 📚 [Examples](./5-example.md) – Hands-on use cases with real-world setups.

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
