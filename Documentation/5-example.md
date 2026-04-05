## Hands-On Examples

Get started quickly with practical examples available in the [Samples](../Samples/NotoriousTests.InfrastructuresSamples/) folder. These examples demonstrate how to set up and use NotoriousTests for real-world scenarios.

### Basic example with SQL Server and a Web API

Let's test this feature of the application with **NotoriousTest**:

```csharp
[HttpPost]
public void CreateUser()
{
    using (var connection = new SqlConnection(_configuration.GetConnectionString("SqlServer")))
    {
        connection.Open();
        CreateUser(connection);
    }
}

private void CreateUser(SqlConnection sqlConnection)
{
    using (var command = sqlConnection.CreateCommand())
    {
        command.Parameters.AddWithValue("@username", "test");
        command.Parameters.AddWithValue("@email", "example@email.com");
        command.Parameters.AddWithValue("@password_hash", "password");
        command.Parameters.AddWithValue("@created_at", DateTime.Now);
        command.CommandText = "INSERT INTO Users(username, email, password_hash, created_at) VALUES(@username, @email, @password_hash, @created_at);";
        command.ExecuteNonQuery();
    }
}
```

### Setup

Create an xUnit Test Project. Then install the required packages:

```sh
dotnet add package NotoriousTest
dotnet add package NotoriousTest.SqlServer
```

### Create and populate the database

```csharp
public class SqlServerInfrastructure : SqlServerContainerInfrastructure
{
    public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry)
        : base(contextId, logger, registry) { }

    // Publish the connection string under the key the web app expects
    protected override string ConnectionStringKey => "ConnectionStrings:SqlServer";

    public override async Task Initialize()
    {
        await base.Initialize();

        // Run schema migrations after the container and database are ready
        await using var connection = GetDatabaseConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = @"CREATE TABLE Users (
            user_id INT IDENTITY(1,1) PRIMARY KEY,
            username NVARCHAR(50) NOT NULL UNIQUE,
            email NVARCHAR(100) NOT NULL UNIQUE,
            password_hash NVARCHAR(255) NOT NULL,
            created_at DATETIME DEFAULT GETDATE()
        );";
        await command.ExecuteNonQueryAsync();
    }
}
```

### Create the WebApplication

```csharp
public class SampleProjectApp : WebApplication<Program> { }
```

### Create the environment

```csharp
public class TestEnvironment : NotoriousTest.XUnit.Environment
{
    public TestEnvironment(IMessageSink sink) : base(sink) { }
    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment()
    {
        AddInfrastructure<SqlServerInfrastructure>();
        this.AddWebApplication<SampleProjectApp>();
    }
}
```

### Run the tests

```csharp
public class SampleTests : IntegrationTest<TestEnvironment>
{
    public SampleTests(TestEnvironment environment) : base(environment) { }

    [Fact]
    public async Task Test1()
    {
        HttpClient client = CurrentEnvironment.GetWebApplication().HttpClient;
        HttpResponseMessage response = await client.PostAsync("users", null);
        Assert.True(response.IsSuccessStatusCode);

        SqlServerInfrastructure sqlInfrastructure = CurrentEnvironment.GetInfrastructure<SqlServerInfrastructure>();

        await using (DbConnection sql = sqlInfrastructure.GetDatabaseConnection())
        {
            await sql.OpenAsync();
            using (DbCommand command = sql.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Users";
                int count = (int)await command.ExecuteScalarAsync();
                Assert.Equal(1, count);
            }
        }
    }
}
```

### What's included in the Samples folder:

- **[SqlServerInfrastructure.cs](../Samples/NotoriousTests.InfrastructuresSamples/Infrastructures/SqlServerInfrastructure.cs)**
  Learn how to manage your SQL Server database using Respawn, TestContainers, and plain SQL for creating, destroying, and resetting your database seamlessly.
- **[TestWebApplication.cs](../Samples/NotoriousTests.InfrastructuresSamples/Infrastructures/TestWebApplication.cs)**
  See how to configure a WebApplicationFactory with in-memory configuration for fast and isolated tests.
- **[TestEnvironment.cs](../Samples/NotoriousTests.InfrastructuresSamples/Environments/TestEnvironment.cs)**
  Understand how to set up environments to manage multiple infrastructures effortlessly.
- **[SampleTests.cs](../Samples/NotoriousTests.InfrastructuresSamples/SampleTests.cs)**
  Dive into this file to see how to access infrastructures and use them in your tests.
- **[Program.cs](../Samples/NotoriousTests.InfrastructuresSamples.TestWebApp/Program.cs)**
  Explore how the SqlServerInfrastructure generates configuration for the Web Application.

## ✅ Next Steps

Now that your first test is running, explore more features:

- 📖 [Core Concepts](./2-core-concepts.md) – Learn how infrastructures and environments work.
- ⚡ [Advanced Features](./3-advanced-features.md) – Discover ordering, reset behaviors, extensions, DI, and logging.
- 🔌 [Supported Infrastructures](./4-integrations.md) – See how to integrate SQL Server, TestContainers, and more.

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
