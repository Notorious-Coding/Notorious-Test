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

## 3.1.0 

### ✨ Features

- Added PostgreSql integration

### 🛠 Technical

- `GetInfrastructuresAsync` in AsyncEnvironment is no longer Async
- Migrate to slnx
- Extended target frameworks: NotoriousTest now builds for .NET 6, .NET 8, and .NET 9 (previously only .NET 6)

## 4.0.0

### 🛠 Technical

- Synchronous classes are no longer available, all async classes have been renamed.
- .NET 6 is no longer supported.
- NotoriousTest is now in .net standard 2.1, web support has been moved to NotoriousTest.Web