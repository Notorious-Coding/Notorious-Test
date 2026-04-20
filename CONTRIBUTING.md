# Contributing to NotoriousTest

Thank you for your interest in contributing to **NotoriousTest**! All contributions are welcome, whether it's bug reports, feature requests, documentation improvements, or code changes.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Submitting Pull Requests](#submitting-pull-requests)
- [Development Setup](#development-setup)
- [Coding Conventions](#coding-conventions)
- [Testing](#testing)
- [Releasing](#releasing)

---

## Code of Conduct

Please be respectful and constructive in all interactions. We want this project to be a welcoming place for everyone.

---

## Getting Started

1. **Fork** the repository on GitHub.
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/<your-username>/Notorious-Test.git
   cd Notorious-Test
   ```
3. **Create a branch** for your change:
   ```bash
   git checkout -b feature/my-feature
   ```

---

## Project Structure

```
NotoriousTest/               # Orchestration package
NotoriousTest.Core/          # Main package (core abstractions)
NotoriousTest.Runtime/       # Lifecycle runtime
DoggyDog/                    # Watchdog — cleanup after unexpected crashes
NotoriousTest.{XUnit,NUnit,MSTest,TUnit}/   # Test framework adapters
NotoriousTest.{Database,Sqlite,SqlServer,PostgreSql,TestContainers,Web}/  # Infrastructure implementations
Samples/                     # Example projects per framework
Documentation/               # In-depth guides
```

---

## How to Contribute

### Reporting Bugs

Use the [Bug Report template](https://github.com/Notorious-Coding/Notorious-Test/issues/new?template=bug_report.yml) when opening an issue — it will guide you through the required information (steps to reproduce, expected vs. actual behaviour, .NET version, test framework, NotoriousTest version, minimal reproduction project).

### Suggesting Features

Use the [Feature Request template](https://github.com/Notorious-Coding/Notorious-Test/issues/new?template=feature_request.yml) when opening an issue — it will prompt you for the problem you are solving, your proposed solution, alternatives considered, and the affected scope.

### Submitting Pull Requests

1. Make sure an issue exists (or open one) so we can discuss the change before you invest significant time.
2. Keep PRs focused — one concern per PR.
3. Ensure all tests pass locally before opening the PR.
4. Fill in the PR description with context and a link to the related issue.

### Definition of Done

A PR is considered ready to merge when all of the following are true:

- [ ] Unit tests added or updated.
- [ ] Integration tests added or updated.
- [ ] Samples are still functional.
- [ ] Documentation updated (`README.md` and/or files in `Documentation/`).
- [ ] `CHANGELOG.md` updated under the appropriate version heading.

---

## Development Setup

**Prerequisites:**

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later.
- Docker (required for TestContainers-based tests).

**Build everything:**

```bash
dotnet build
```

**Run all tests:**

```bash
dotnet test
```

---

## Coding Conventions

This project follows the C# conventions defined in [`.editorconfig`](.editorconfig):

- Constants use `SCREAMING_SNAKE_CASE`.
- Follow the existing style in the file you are editing — consistency matters more than personal preference.
- No unnecessary comments; let well-named identifiers speak for themselves.
- Do not add error handling for scenarios that cannot happen; trust framework guarantees.
- Use `dotnet format` to apply code styles if necessary.

---

## Testing

- Each new infrastructure or feature must be covered by integration tests.
- Tests should be fully isolated — no shared mutable state between test runs.

---

## Running DoggyDog Manually

DoggyDog is normally spawned automatically by the test framework. When working on DoggyDog itself (or debugging the watchdog lifecycle), you can launch it manually.

### 1. Enable manual launch mode in your test project

In your `testsettings.json`, set `ManualLaunch` to `true`:

```json
{
  "Environment": {
    "DisableWatchdog": false
  },
  "Watchdog": {
    "ManualLaunch": true
  }
}
```

When this flag is set, NotoriousTest will **not** spawn DoggyDog automatically. Instead, it will write the required parameters as user-scoped environment variables (`DOGGYDOG_DEBUG_*`) and then poll for a running `DoggyDog` process every 5 seconds before proceeding with the tests.

### 2. Start DoggyDog from your IDE

Two launch profiles are already defined in [`DoggyDog/Properties/launchSettings.json`](DoggyDog/Properties/launchSettings.json):

| Profile | Command line | When to use |
|---|---|---|
| **With env params** | `--from-env` | Use together with `ManualLaunch: true` — reads the `DOGGYDOG_DEBUG_*` env vars written by NotoriousTest automatically |
| **With arguments** | full argument list | Use when you want to supply every parameter manually (hardcoded paths, specific PID, etc.) |

Select the appropriate profile in Visual Studio / Rider and hit **Run** or **Debug**.

If you prefer the command line, the equivalent invocations are:

```bash
# Option A — read from environment variables (requires ManualLaunch: true)
DoggyDog --from-env

# Option B — direct arguments
DoggyDog \
  --pid <test-runner-pid> \
  --assembly "<path-to-test-assembly.dll>" \
  --connectionString "Data Source=<path-to-registry.db>" \
  --environment "<environment-guid>" \
  [--runtimes "<runtime-path-1>|<runtime-path-2>"] \
  [--loglevel Debug]
```

| Argument | Required | Description |
|---|---|---|
| `--pid` | Yes | PID of the test runner process to monitor |
| `--assembly` | Yes | Full path to the test assembly (`.dll`) |
| `--connectionString` | Yes | SQLite connection string to the infrastructure registry |
| `--environment` | Yes | Environment GUID that identifies the test session |
| `--runtimes` | No | Pipe-delimited list test project runtimes |
| `--loglevel` | No | `Error`, `Warning`, `Success`, `Info` (default), `Trace`, or `Debug` |

### 3. Attach a debugger (optional)

With `ManualLaunch: true` the tests will wait until a `DoggyDog` process is detected. Start DoggyDog with the **With env params** profile in debug mode — your IDE will attach automatically before the test session proceeds.

### Disabling DoggyDog entirely

If you want to run tests without any watchdog (e.g., in a restricted environment without SQLite or Docker), set `DisableWatchdog: true`:

```json
{
  "Environment": {
    "DisableWatchdog": true
  }
}
```

---

## Releasing

Releases are handled by the maintainer via the GitHub Actions workflows:

- **Pre-release:** `.github/workflows/prerelease.yml`: Triggered by pull request and push to develop
- **Release:** `.github/workflows/release.yml`: Triggered by github releases.

Contributors do not need to worry about releases; just make sure `CHANGELOG.md` is updated in your PR.

Do not hesitate to test prerelease workflow changes with [act](https://github.com/nektos/act).
---

Questions? Start a [discussion](https://github.com/Notorious-Coding/Notorious-Test/discussions) or open an issue.
