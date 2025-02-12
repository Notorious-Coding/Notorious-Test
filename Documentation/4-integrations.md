## Integrations

- [TestContainers](#testcontainers)
- [SqlServer](#sql-server)

### TestContainers

**NotoriousTest.TestContainers** is now available as a separate package.

Install [NotoriousTest](https://www.nuget.org/packages/NotoriousTest/) from the package manager console:

```
PM> Install-Package NotoriousTest.TestContainers
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest.TestContainers
```

This package provides classes that automatically start and stop the container at the beginning and end of the test campaign!
It introduces three new classes:

- `DockerContainerAsyncInfrastructure<TContainer>` : Standard infrastructure.
- `ConfiguredDockerContainerAsyncInfrastructure<TContainer>`: Infrastructure handling configuration as a dictionary.
- `ConfiguredDockerContainerAsyncInfrastructure<TContainer, TConfiguration>` : Infrastructure handling a configuration object.

> ❗ Since `TestContainers` doesn't support synchronous code, theses classes are only available in an `AsyncEnvironment`.

Here's an example :

```csharp

public class SqlServerContainerInfrastructure : DockerContainerAsyncInfrastructure<GenericContainer>
{
    public override Container {get; init;} = new MsSqlBuild().Build();

    public SampleDockerContainer(bool initialize = false) : base(initialize)
    {
    }

    public override Task Reset()
    {
        return Task.CompletedTask;
    }
}
```

### Sql Server

#### Initialization

**NotoriousTest.SqlServer** is now available as a separate package.

Install [NotoriousTest.SqlServer](https://www.nuget.org/packages/NotoriousTest.SqlServer/) from the package manager console:

```
PM> Install-Package NotoriousTest.SqlServer
```

Or from the .NET CLI as:

```
dotnet add package NotoriousTest.SqlServer
```

You can now simply use the SqlServerContainerAsyncInfrastructure to start a SQL Server database.

It will automatically start at the beginning of the test campaign, stop at the end, and reset between each test, powered by TestContainers and Respawn.

Here's an example:

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }
    }
```

This infrastructure performs several tasks:

- **On initialization:**
  - Starts a SQL Server Docker container, powered by TestContainers.
  - Creates a unique database.
- **On reset:**
  - Empties the database, powered by Respawn.
- **On destruction:**
  - Stops the container.

#### Test Usage

The infrastructure provides two methods:

- **`GetDatabaseConnection`** – Returns a `SqlConnection` pointing to the newly created database for your test.
- **`GetDatabaseConnectionString`** – Returns a connection string pointing to the newly created database for your test.

```csharp
[Fact]
public async Task Test1()
{
    SqlServerInfrastructure sqlInfrastructure = await CurrentEnvironment.GetInfrastructureAsync<SqlServerInfrastructure>();
    await using(SqlConnection sql = sqlInfrastructure.GetDatabaseConnection())
    {
        // Arrange your database here.
    }
}
```

#### Populating the database

You can populate the database by overriding the `PopulateDatabase` method :

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        protected override async Task PopulateDatabase(SqlConnection connection)
        {
            // Play all your migrations script here, use DBUp or any other migration tool
            await CreateTables(connection);
        }
    }
```

#### Generating configuration

You can generate configuration for your web application by overriding the `Initialize` method :

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            // We can add the connection string to the configuration.
            Configuration.Add("ConnectionStrings:SqlServer", GetDatabaseConnectionString());
        }
    }
```

#### Configure the container

You can configure the container by overriding the `ConfigureSqlContainer` method :

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
        }

        protected override MsSqlBuilder ConfigureSqlContainer(MsSqlBuilder builder)
        {
            // Configure the builder to override image, host, password, port, etc.
            return builder.WithPassword("NotoriousStrong(!)Password6");
        }
    }
```

#### Configure respawn settings

To configure respawn settings, you can assign RespawnOptions property. They will be used by respawn to reset the database.

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
            RespawnOptions = new RespawnerOptions
            {
                TablesToIgnore = new Table[] { "MyMigrationTable" }
            };
        }
    }
```

#### Configure the database name

By default, Database name will be `NotoriousDb` concatened with the `ContextId`.
You can override `NotoriousDb` by your custom name by setting the property `DbName`.

```csharp
   public class SqlServerInfrastructure : SqlServerContainerAsyncInfrastructure
    {
        public SqlServerInfrastructure()
        {
            DbName = "TestDb";
        }
    }
```
