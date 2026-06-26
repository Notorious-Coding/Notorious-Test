# Changelog

## v5.0.1

### 🐛 Bug Fixes

- Fixed a bug where internal packages were not available at installation. Preventing NotoriousTest from being installed.
- All internal packages are now available to download from Nuget.org, but every class is internal. Which makes them
  impossible to use.

## v5.0.0

### ✨ Features

- Dependency injection is now at the Environment level
    - Create a `IDependencyInjectionConfigurator`
      ```csharp
      public class DependencyInjectionConfigurator : IDependencyInjectionConfigurator
      {
          public IServiceCollection ConfigureServices(IServiceCollection services) => services;
      }
      ```
    - Add a `[InjectionConfigurator(typeof(DependencyInjectionConfigurator))]` to the test class.
        - You could add multiple `InjectionConfiguratorAttribute` to the same test class (or parents), they will all be
          used.
    - Add dependencies to the environment/infrastructure constructor !

### 💥 Breaking Changes

- Deleted support for Extensions, use dependency injection instead.
-

### 🐛 Bug Fixes

- Fixed a bug where SqliteInfrastructure could not delete the file because it was still used by another process.
  Triggering DoggyDog cleaning.

### 📦 Dependencies

- Upgraded 'Microsoft.Data.Sqlite' and 'Microsoft.Data.Sqlite.Core' to 11.0.0-preview.5.26302.115 because 10.0.5 has a
  high
  vulnerability on SQlite provider.

## v4.1.1

### 🛠 Technical

- Internal packages (such as NotoriousTest.Runtimes|Watchdog|SqliteRegistry|TestSettings) are now includes in
  NotoriousTest.
  And can no longer be downloaded via Nuget Packages.

## v4.1.0

### ✨ Features

- DoggyDog now log when an infrastructure is initialized, reset, or destroyed normally.
- You can now use `testsettings.json` to disable DoggyDog for the entire test project.

```json
{
    "Environment": {
      "DisableWatchdog": false
    }
}
```

### 🛠 Technical

- For debugging purpose, the test suite can now wait for DoggyDog to be launched manually.
- Enable this mode by setting `ManualLaunch` to true in the `testsettings.json` file.

```json
{
  "Watchdog": {
    "ManualLaunch": false
  }
}
```

By doing this, the environment will set parameters trough User Environment Variables.
Launch DoggyDog with the --from-env flag to read those parameters.

## v4.0.2

### 🐛 Bug Fixes

- Fixed a bug where DoggyDog could not resolve shared frameworks assembly, preventing cleaners from being executed.

## v4.0.1

### 🐛 Bug Fixes

- Fixed a bug where internal packages of NotoriousTest were not available at installation.

## v4.0.0

### ✨ Core features

#### DoggyDog 🐶🐶🐶 💥NEW💥

NotoriousTest now has a new mascot, the DoggyDog ! 🐶🐶🐶
DoggyDog is a watchdog that clean infrastructures that may have been left dirty by previous tests,
and make sure that your tests are running in a clean environment.

- Introducing DoggyDog - an executable that clean your infrastructures left behind a test campaign that have been killed
  unexpectedly.
- DoggyDog use a registry to track all your infrastructures, and execute their cleaner.
- [Cleaner(CleanerType)] attribute to specify the cleaner of your infrastructures.

#### Infrastructure extensions 💥NEW💥

**Infrastructure extensions** introduce a composition model for adding behaviors to your infrastructures. Instead of
creating specialized subclasses to combine multiple concerns (configuration, database seeding, respawn, etc. )
You register extensions on any infrastructure via `EnsureExtension<TExtension>()`.
Each extension is self-contained, reusable across infrastructures, and hooks into the infrastructure lifecycle through
dedicated callbacks such as `OnBeforeInitialize`.

- Introducing a new concept called infrastructure extension, meant to be used to react to infrastructure setup.
- New interface `IInfrastructureExtension`, provide hooks such as `OnBeforeInitialize` to extends Infrastructure.
- Configuration is now handled by extensions classes.
- Use `EnsureExtension<MyExtension>()` or `EnsureExtension(new MyExtension())`  to register an extension.
- Built-in extensions :
    - Core
        - `OutputConfigurationExtension<TOutputConfiguration>` : Provide a way to output configuration. Included in
          `Infrastructure` base class.
        - `SettingsExtension<TSettings>`: Load from `testsettings.json` your infrastructure configuration. Config key
          default to infrastructure name, and can be override.
    - Database
        - `RespawnExtension`: Integration of Respawn package to reset databases between test.
        - Included in every database infrastructures by default.

#### Settings 💥NEW💥

- By registering a `SettingsExtension`, you can now load settings from `testsettings.json` to configure infrastructure.
- Automatically loaded from the infrastructure name as config key.

#### Output configuration 🔧 UPDATED 🔧

- Environments no longer require a global configuration object. Configuration is now propagated automatically through
  extensions.
- Output configuration is now handled by an extension built-in Infrastructure base class.
- Adding a configuration output will now be made by calling `AddEntry(key, config)`.
- Environment will gather all configuration under all keys and pass to all `IConfigurationConsumer` infrastructures,
  such as `WebApplicationInfrastructure`.
- `WebApplicationInfrastructure` now maps configuration entries to appsettings format automatically. Generating the
  section path from the key and config structure.
- e.g.

```json
  // Entry: "Example:Test" → { "Host": "localhost", "Port": 5432 }
  // appsettings.json
  {
    "Example": {
		"Test": {
			"Host": "localhost",
			"Port": 5432
		}
      }
  }
```

#### Database 💥NEW💥

- Non docker database infrastructure for SqlServer and PostgreSql have been added ! For those who want to run test on
  existing servers.
- New package NotoriousTest.Database provides base classes for docker and non-docker database infrastructures.
- `ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings>`: Base class for non docker database infrastructures,
  that need a running server to setup.
    - `DatabaseSettings` will be loaded directly from the `testsettings.json` file.
- `DockerDatabaseInfrastructure<TContainer>`: Base class for testcontainers powered infrastructure. Takes a
  `IDatabaseContainer`.
- SqlServer, PostgreSql and Sqlite packages are using theses classes.

### Dependency Injection 💥NEW💥

- Environments now expose an internal DI container, configurable via ConfigureInfrastructureServices(IServiceCollection
  collection).
- Registered services are automatically injected into every infrastructure, removing the need for manual wiring in
  Initialize.
- Available by default: ContextId, ITestSettingsProvider.

#### Logging 💥NEW💥

- NotoriousTest now deliver a `ITestLogger` that is injected directly in the `Infrastructure.Logger` property, and can
  be accessed from everywhere via DI.
- Every framework has it's own `ITestLogger` implementation that is registered in DI.

#### Web

- Web support has been moved to `NotoriousTest.Web`.
- `Environment` now has extensions to retrieve the WebApplication or add a WebApplication. You can find them in the
  `NotoriousTest.Web` packages, under the `NotoriousTest.Web.Environment.WebEnvironmentExtensions`.
- `WebEnvironment` has been deleted. Use extensions to retrieve/add WebApplication.

### 💥 Breaking Changes

- Synchronous classes have been removed. All `Async`-prefixed classes have been renamed without the suffix (e.g.
  `AsyncInfrastructure` → `Infrastructure`).
- .NET 6 is no longer supported. Minimum target is .NET 8.
- XUnit has been updated to xunit.v3, which introduces breaking changes of its own. See
  the [xunit.v3 migration guide](https://xunit.net/docs/getting-started/v3/migration).
- Infrastructures are now executed in parrallel if there order are the same. IConfigurationConsumer are run after all
  infrastructures with the same order.

### Integrations

#### Sqlite integration 💥NEW💥

- `SqliteInfrastructure` is now available in the `NotoriousTest.Sqlite` package.

#### Test Frameworks integration 💥NEW💥

- NotoriousTest is now compatible with **NUnit**, **MSTest**, **TUnit**, in addition to **xUnit**.
- Find them within `NotoriousTest.XUnit`, `NotoriousTest.NUnit`, `NotoriousTest.MSTest` and `NotoriousTest.TUnit`
  packages.
- New samples for every frameworks are available in the samples folder.

## v3.1.0

### ✨ Features

- Added PostgreSql integration

### 🛠 Technical

- `GetInfrastructuresAsync` in AsyncEnvironment is no longer Async
- Migrate to slnx
- Extended target frameworks: NotoriousTest now builds for .NET 6, .NET 8, and .NET 9 (previously only .NET 6)

## v3.0.0

### ✨ Features

- `ConfiguredInfrastructure` and `AsyncConfiguredInfrastructure` are replaced with `IConfigurableInfrastructure`
  interfaces. Every infrastructures can be marked as configurable just by implementing this interface.

### 🛠 Technical

- Added C4 model architecture schema

## v2.3.1

### 🐛 Bug Fixes

- Fixed a bug in **NotoriousTest.SqlServer** where the `SqlServerContainerAsyncInfrastructure` did not changes the
  database connection to point to the newly created database.

## v2.3.0

### ✨ Features

- **NotoriousTest.TestContainers** is now available as a separate package.
    - Provides a simple way to use TestContainers in your tests.
    - For more information, see the [Advanced Functionalities - TestContainers](./README.md#testcontainers).
- **NotoriousTest.SqlServer** is now available as a separate package.
    - Provide your tests with a SqlServer ready-to-use infrastructure !
    - For more information, see the [Advanced Functionalities - SqlServer](./README.md#sql-server).

### 🛠 Technical

- Simplified management of generic types in the `AsyncConfiguredInfrastructure` and `AsyncConfiguredEnvironment`
  classes.

## v2.2.0

### ✨ Features

- Introduced `ContextId` to uniquely identify infrastructures. For example, you can name your database with it.
    - In standalone mode, `ContextId` will be a random GUID.
    - Within an Environment, `ContextId` will be the environment identifier `Environment.EnvironmentId`
- Removal of `IConfigurationProducer` and `IConfigurationConsumer`, as it was not necessary.
- `Order` property is now nullable and thus optional.

### 🛠 Technical

- Implemented multiple unit tests to enhance reliability.
- Several changes to improve consistency in package usage (naming, methods, etc.).

### 🐛 Bug Fixes

- Fixed a bug where `EnvironmentId` generated a new GUID on every reference.
- Fixed a bug where the configuration was erased when using an object as the configuration in
  `AsyncConfiguredEnvironment<TConfig>`.

## v2.1.0

### ✨ Features

- Added the `AutoReset` property to toggle infrastructure reset on or off.

For more information, see
the [Advanced Functionalities - Advanced control over Infrastructure Reset](./README.md#advanced-control-over-infrastructure-resets)

## v2.0.0

### ✨ Features :

- **Complete overhaul of configuration management:**
    - **`AsyncConfiguredInfrastructure`** and **`AsyncConfiguredInfrastructure<TConfig>`**: Provides access to the
      `Configuration` property via an infrastructure.
    - **`IConfigurationConsumer`** and **`IConfigurationProducer`**: Used to indicate whether a component consumes or
      produces configuration.
    - **`AsyncConfiguredEnvironment`**: An environment managing the provisioning of a global configuration from
      configuration infrastructures.
    - **`WebApplication`** is now automatically provided with configuration by the `AsyncWebEnvironment`.

For more information, see the [Advanced Functionalities - Configuration](./README.md#configuration)
and [Advanced Functionalities - Web](./README.md#web).
