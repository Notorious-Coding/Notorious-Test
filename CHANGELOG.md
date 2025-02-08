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

### 🛠 Technical

- Implemented multiple unit tests to enhance reliability.
