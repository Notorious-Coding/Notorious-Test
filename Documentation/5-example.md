## Hands-On Examples

Get started quickly with practical examples available in the [Samples](./Samples/NotoriousTests.InfrastructuresSamples/) folder. These examples demonstrate how to set up and use NotoriousTests for real-world scenarios.

### Basic example with Sql Server and a Web API.

Lets test this feature of my application with **NotoriousTest** :

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

Create a xUnit Test Project.
First, [install NuGet](http://docs.nuget.org/docs/start-here/installing-nuget). Then, install [NotoriousTest](https://www.nuget.org/packages/NotoriousTest/) from the .NET CLI as:

```sh
dotnet add package NotoriousTest
```

### Create and populate the database

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        protected override async Task PopulateDatabase(SqlConnection connection)
        {
            // Play all your migrations script here, use DBUp or any other migration tool
            using (SqlCommand command = connection.CreateCommand())
            {
                string sql = @"CREATE TABLE Users (
                                    user_id INT IDENTITY(1,1) PRIMARY KEY,
                                    username NVARCHAR(50) NOT NULL UNIQUE,
                                    email NVARCHAR(100) NOT NULL UNIQUE,
                                    password_hash NVARCHAR(255) NOT NULL,
                                    created_at DATETIME DEFAULT GETDATE()
                                );
                               ";
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync();
            }
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            // We can add the connection string to the configuration.
            Configuration.Add("ConnectionStrings:SqlServer", GetDatabaseConnectionString());
        }
    }
```

### Create the WebApplication

```csharp
    public class SampleProjectApp : WebApplication<Program>{}
```

### Create the environment

```csharp
    public class TestEnvironment : AsyncWebEnvironment<Program>
    {
        public override async Task ConfigureEnvironmentAsync()
        {
            await AddInfrastructure(new SqlServerInfrastructure());
            await AddWebApplication(new TestWebApplication());
        }
    }
```

### Run the tests

```csharp
[Fact]
public async Task Test1()
{
    HttpClient client = (await CurrentEnvironment.GetWebApplication()).HttpClient;
    HttpResponseMessage response = await client.PostAsync("users", null);
    Assert.True(response.IsSuccessStatusCode);

    SqlServerInfrastructure sqlInfrastructure = await CurrentEnvironment.GetInfrastructureAsync<SqlServerInfrastructure>();

    await using(SqlConnection sql = sqlInfrastructure.GetDatabaseConnection())
    {
        await sql.OpenAsync();
        using (SqlCommand command = sql.CreateCommand())
        {
            command.CommandText = "SELECT COUNT(*) FROM Users";
            int count = (int)await command.ExecuteScalarAsync();
            Assert.Equal(1, count);
        }
    }
}
```

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
  Explore how the SqlServerInfrastructure generates configuration for the Web Application.\*

## ✅ Next Steps

Now that your first test is running, explore more features:

- 📖 [Core Concepts](./2-core-concepts.md) – Learn how infrastructures and environments work.
- ⚡ [Advanced Features](./3-advanced-features.md) – Discover ordering, reset behaviors, and more. \
- 🔌 [Supported Infrastructures](./4-integrations.md) – See how to integrate SQL Server, TestContainers, and more.

💡 Need help or have feedback? Join the community [discussions](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an [issue](https://github.com/Notorious-Coding/Notorious-Test/issues) on GitHub.
