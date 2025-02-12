## ![Logo](./Documentation/Images/NotoriousTest.png)

[![NuGet](https://img.shields.io/nuget/v/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NotoriousTest)](https://www.nuget.org/packages/NotoriousTest/)
[![License](https://img.shields.io/github/license/Notorious-Coding/Notorious-Test)](https://github.com/Notorious-Coding/Notorious-Test/blob/master/LICENSE.txt)
[![.NET](https://img.shields.io/badge/.NET-6%2B-blue)](https://dotnet.microsoft.com/)
[![GitHub stars](https://img.shields.io/github/stars/Notorious-Coding/Notorious-Test?style=social)](https://github.com/Notorious-Coding/Notorious-Test/stargazers)

Clean, isolated, and maintainable integration testing for .NET

## Contact

Have questions, ideas, or feedback about NotoriousTests?
Feel free to reach out! I'd love to hear from you. Here's how you can get in touch:

- GitHub Issues: [Open an issue](https://github.com/Notorious-Coding/Notorious-Test/issues) to report a problem, request a feature, or share an idea.
- Email: [briceschumacher21@gmail.com](mailto:briceschumacher21@gmail.com)
- LinkedIn : [Brice SCHUMACHER](http://www.linkedin.com/in/brice-schumacher)

The discussions tabs is now opened !
Feel free to tell me if you use the package here : https://github.com/Notorious-Coding/Notorious-Test/discussions/1 !

## Summary

- [The problem with integration testing in .NET](#-the-problem-with-integration-testing-in-net)
- [NotoriousTest: The solution](#-notorioustest-the-solution)
- [Why use NotoriousTest](#why-use-notorious-test)
- [Getting started](#getting-started)
- [Resources & Community](#ressources-&-community)
  - [Changelog](#changelog)
  - [Contact](#contact)

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

## Quickstart

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

## Changelog

You can find the changelog [here](./CHANGELOG.md).

## Other nugets i'm working on

- [**NotoriousClient**](https://www.nuget.org/packages/NotoriousClient/) : Notorious Client is meant to simplify the sending of HTTP requests through a fluent builder and an infinitely extensible client system.
- [**NotoriousModules**](https://github.com/Notorious-Coding/Notorious-Modules) : Notorious Modules provide a simple way to separate monolith into standalone modules.
