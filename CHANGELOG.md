# Changelog

## v2.0.0

### ✨ Features :

- **Complete overhaul of configuration management:**
  - **`AsyncConfiguredInfrastructure`** and **`AsyncConfiguredInfrastructure<TConfig>`**: Provides access to the `Configuration` property via an infrastructure.
  - **`IConfigurationConsumer`** and **`IConfigurationProducer`**: Used to indicate whether a component consumes or produces configuration.
  - **`AsyncConfiguredEnvironment`**: An environment managing the provisioning of a global configuration from configuration infrastructures.
  - **`WebApplication`** is now automatically provided with configuration by the `AsyncWebEnvironment`.

For more information, see the [Advanced Functionalities - Configuration](./README.md#configuration) and [Advanced Functionalities - Web](./README.md#web).

## v2.1.0

### ✨ Features

- Added the `AutoReset` property to toggle infrastructure reset on or off.

For more information, see the [Advanced Functionalities - Advanced control over Infrastructure Reset](./README.md#advanced-control-over-infrastructure-resets)

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
- Fixed a bug where the configuration was erased when using an object as the configuration in `AsyncConfiguredEnvironment<TConfig>`.

## v2.3.0

### ✨ Features

- **NotoriousTest.TestContainers** is now available as a separate package.
  - Provides a simple way to use TestContainers in your tests.
  - For more information, see the [Advanced Functionalities - TestContainers](./README.md#testcontainers).
- **NotoriousTest.SqlServer** is now available as a separate package.
  - Provide your tests with a SqlServer ready-to-use infrastructure !
  - For more information, see the [Advanced Functionalities - SqlServer](./README.md#sql-server).

### 🛠 Technical

- Simplified management of generic types in the `AsyncConfiguredInfrastructure` and `AsyncConfiguredEnvironment` classes.

## v2.3.1

### 🐛 Bug Fixes

- Fixed a bug in **NotoriousTest.SqlServer** where the `SqlServerContainerAsyncInfrastructure` did not changes the database connection to point to the newly created database.

## v3.0.0

### ✨ Features

- `ConfiguredInfrastructure` and `AsyncConfiguredInfrastructure` are replaced with `IConfigurableInfrastructure` interfaces. Every infrastructures can be marked as configurable just by implementing this interface.

### 🛠 Technical

- Added C4 model architecture schema

## v3.1.0 

### ✨ Features

- Added PostgreSql integration

### 🛠 Technical

- `GetInfrastructuresAsync` in AsyncEnvironment is no longer Async
- Migrate to slnx
- Extended target frameworks: NotoriousTest now builds for .NET 6, .NET 8, and .NET 9 (previously only .NET 6)

## v4.0.0

### ✨ Features

#### DoggyDog 🐶🐶🐶 💥NEW💥

NotoriousTest now has a new mascot, the DoggyDog ! 🐶🐶🐶
DoggyDog is a watchdog that clean infrastructures that may have been left dirty by previous tests, 
and make sure that your tests are running in a clean environment.

- Introducing DoggyDog - an executable that clean your infrastructures left behind a test campaign that have been killed unexpectedly.
- DoggyDog use a registry to track all your infrastructures, and execute their cleaner.
- [Cleaner(CleanerType)] attribute to specify the cleaner of your infrastructures.

#### Test Frameworks integration 💥NEW💥

- NotoriousTest is now compatible with NUnit, MSTest, TUnit, in addition to xUnit.
- Find integration within NotoriousTest.XUnit, NotoriousTest.NUnit, NotoriousTest.MSTest and NotoriousTest.TUnit packages.

#### Infrastructure extensions 💥NEW💥
- Introducing a new concept called infrastructure extension, meant to be used to react to infrastructure setup. 
- New interface `IInfrastructureExtension`, provide hooks such as `OnBeforeInitialize` to extends Infrastructure.
- Configuration is now handled by extensions classes.
- Use `EnsureExtension<MyExtension>()` or `EnsureExtension(new MyExtension())`  to register an extension.
- Built-in extensions : 
	- Core 
		- `OutputConfigurationExtension<TOutputConfiguration>` : Provide a way to output configuration. Included in `Infrastructure` base class.
		- `SettingsExtension<TSettings>`: Load from `testsettings.json` your infrastructure configuration. Config key default to infrastructure name, and can be override.
	- Database
		- `RespawnExtension`: Integration of Respawn package to reset databases between test.

#### Settings 💥NEW💥
- By registering a `SettingsExtension`, you can now load settings from `testsettings.json` to configure infrastructure.
git stash- Automatically loaded from the infrastructure name as config key.

#### Output configuration 🔧 UPDATED 🔧
- Environments no longer require a global configuration object. Configuration is now propagated automatically through extensions.
- Output configuration is now handled by an extension built-in Infrastructure base class.
- Adding a configuration output will now be made by calling `AddEntry(key, config)`.
- Environment will gather all configuration under all keys and pass to all `IConfigurationConsumer` infrastructures, such as `WebApplicationInfrastructure`.
- `WebApplicationInfrastructure` now maps configuration entries to appsettings format automatically. Generating the section path from the key and config structure. 
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
- Non docker database infrastructure for SqlServer and PostgreSql have been added ! For those who want to run test on existing servers.
- New package NotoriousTest.Database provides base classes for docker and non-docker database infrastructures.
- `ExternalDatabaseInfrastructure<TOutputConfiguration, TSettings>`: Base class for non docker database infrastructures, that need a running server to setup.
	- `DatabaseSettings` will be loaded directly from the `testsettings.json` file.
- `DockerDatabaseInfrastructure<TContainer>`: Base class for testcontainers powered infrastructure. Takes a `IDatabaseContainer`.

#### Dependency Injection 💥NEW💥
- NotoriousTest now handle dependency injection direclty into infrastructures. 
- Configure dependencies via `ConfigurationInfrastructureServices(IServiceCollection collection)` method in your environment ! 

#### Logging 💥NEW💥

- NotoriousTest now deliver a `ITestLogger` that is injected directly in the `Infrastructure.Logger` property, and can be accessed from everywhere via DI.
- Enable diagnostic messages in xunit.runner.json or via xunit attributes, and navigate into the tests output (in visual studio).

#### Web
- Web support has been moved to `NotoriousTest.Web`. 
- `WebEnvironment` no longer need an EntryPoint in generic parameter.
- `WebApplicationInfrastructure` is now available in environment with `WebApp` properties. Use WebApp.HttpClient to make your http calls.

#### Misc
- Synchronous classes have been removed. All `Async`-prefixed classes have been renamed without the suffix (e.g. `AsyncInfrastructure` → `Infrastructure`).
- .NET 6 is no longer supported. Minimum target is .NET 8.
- XUnit has been updated to xunit.v3, which introduces breaking changes of its own. See the [xunit.v3 migration guide](https://xunit.net/docs/getting-started/v3/migration).
- Infrastructures are now executed in parrallel if there order are the same. IConfigurationConsumer are run after all infrastructures with the same order.

