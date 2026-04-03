using AwesomeAssertions;

using Dapper;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.PostgreSql;
using NotoriousTest.SqlLiteRegistry;

using System.Diagnostics;


namespace NotoriousTest.IntegrationTests
{
    public static class DoggyDogTestFramework
    {
        public static class Arrange
        {
            public static Process? StartFakeProcess(int exitCode = 0, int timeToExit = 5)
            {
                return Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c timeout /t {timeToExit} && exit {exitCode}",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                });
            }

            public static async Task CreateRegistry(SqliteInfrastructure registryInfrastructure)
            {
                using var connection = registryInfrastructure.GetDatabaseConnection();
                await connection.ExecuteAsync(SqliteRegistryProvider.ENSURE_REGISTRY);

            }
            public static async Task<InfrastuctureRegistryEntry> PopulateRegistryWithFakeInfrastructure(SqliteInfrastructure registryInfrastructure, Process attachedProcess, Type fakeInfrastructureType)
            {
                using var connection = registryInfrastructure.GetDatabaseConnection();

                var entry = new InfrastuctureRegistryEntry
                {
                    InfrastructureId = Guid.NewGuid(),
                    EnvironmentId = Guid.NewGuid(),
                    ProcessID = attachedProcess.Id,
                    InfrastructureType = fakeInfrastructureType,
                    Metadata = "Test passed !"
                };

                var entity = await connection.QuerySingleAsync<InfrastructureRegistryEntryEntity>(SqliteRegistryProvider.REGISTER_INFRASTRUCTURE, InfrastructureRegistryEntryEntity.FromDomain(entry));
                return entity.ToDomain();
            }

            public class FakeCleaner : IInfrastructureCleaner
            {
                public static string Message = "[FakeCleaner] {0} with {1} for Id : {2}";
                public Task CleanAfterCrash(ContextId contextId, Guid infrastructureId, object? metadata = null)
                {
                    Console.WriteLine(Message, metadata, contextId.Value, infrastructureId);
                    return Task.CompletedTask;
                }
            }
            [Cleaner(typeof(FakeCleaner))]
            public class FakeInfrastructureWithCleaner : Infrastructure<string>
            {
                public FakeInfrastructureWithCleaner(ContextId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
                {
                }

                public override Task Destroy()
                {
                    return Task.CompletedTask;
                }

                public override Task Initialize()
                {
                    return Task.CompletedTask;
                }

                public override Task Reset()
                {
                    return Task.CompletedTask;
                }
            }

            [Cleaner(typeof(FakeCleaner))]
            public class FakeInfrastructure2WithCleaner : Infrastructure<string>
            {
                public FakeInfrastructure2WithCleaner(ContextId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
                {
                }

                public override Task Destroy()
                {
                    return Task.CompletedTask;
                }

                public override Task Initialize()
                {
                    return Task.CompletedTask;
                }

                public override Task Reset()
                {
                    return Task.CompletedTask;
                }
            }

            public class FakeInfrastructureWithoutCleanerAttribute : Infrastructure<string>
            {
                public FakeInfrastructureWithoutCleanerAttribute(ContextId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
                {
                }
                public override Task Destroy()
                {
                    return Task.CompletedTask;
                }
                public override Task Initialize()
                {
                    return Task.CompletedTask;
                }
                public override Task Reset()
                {
                    return Task.CompletedTask;
                }
            }
        }

        public static class Act
        {
            public static Process? StartDoggyDog(int processId, string assembly, string connectionString)
            {
                var doggyDogPath = Path.Combine(AppContext.BaseDirectory, "DoggyDog.exe");
                Process? doggyDogProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = doggyDogPath,
                    Arguments = $"--pid {processId} --assembly \"{assembly}\" --connectionString \"{connectionString}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                });

                doggyDogProcess!.StandardInput.Close();
                return doggyDogProcess;
            }
        }

        public static class Assert
        {
            public static async Task ShouldHaveFoundInfrastructureToClean(string stdout, int count, int processId)
            {
                stdout.Should().Contain($"[DoggyDog] {count} infrastructure(s) registered for PID {processId}. Starting cleanup...");
            }
            public static async Task ShouldHaveNotFoundInfrastructureToClean(string stdout, int processId)
            {
                stdout.Should().Contain($"[DoggyDog] No registered infrastructure found for PID {processId}. Nothing to clean up.");
            }
            public static async Task ShouldHaveCleanedEntry(string stdout, SqliteInfrastructure registry, InfrastuctureRegistryEntry entry)
            {
                using var connection = registry.GetDatabaseConnection();
                var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM InfrastructureRegistry WHERE ProcessID = @ProcessId", new { ProcessId = entry.ProcessID });

                count.Should().Be(0);

                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleanup in progress...");
                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleaning using {nameof(Arrange.FakeCleaner)}");
                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleanup successful. Removing registry entry...");
                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Registry entry removed.");
            }

            public static async Task ShouldHaveNotFoundCleanerAttribute(string stdout, SqliteInfrastructure registry, InfrastuctureRegistryEntry entry)
            {
                using var connection = registry.GetDatabaseConnection();
                var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM InfrastructureRegistry WHERE ProcessID = @ProcessId", new { ProcessId = entry.ProcessID });

                count.Should().Be(1);

                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleanup in progress...");
                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] No [Cleaner] attribute found. Skipping.");
            }

            public static async Task ShouldHaveNotFoundCleaner(string stdout, SqliteInfrastructure registry, InfrastuctureRegistryEntry entry)
            {
                using var connection = registry.GetDatabaseConnection();
                var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM InfrastructureRegistry WHERE ProcessID = @ProcessId", new { ProcessId = entry.ProcessID });

                count.Should().Be(1);

                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleanup in progress...");
                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Failed to instantiate cleaner. Skipping.");
            }

            public static async Task ShouldHaveInitiatedRecovery(string stdout, Process? attachedProcess)
            {
                stdout.Should().Contain($"[DoggyDog] Process {attachedProcess.Id} exited with code {attachedProcess.ExitCode}. Initiating crash recovery...");
            }

            public static async Task ShouldHaveMonitoredProcess(string stdout, Process? attachedProcess)
            {
                stdout.Should().Contain($"[DoggyDog] Monitoring PID {attachedProcess.Id} - awaiting termination...");
            }

            public static async Task ShouldHaveFoundRegistryFile(string stdout, string registryPath)
            {
                stdout.Should().Contain($"[DoggyDog] Using registry file at {registryPath}");
            }

            public static async Task ShouldHaveNotFoundRegistryFile(string stdout, string registryPath)
            {
                stdout.Should().Contain($"[DoggyDog] Registry file not found at {registryPath}");
            }

            public static async Task ShouldHaveExitNormally(string stdout, Process? attachedProcess)
            {
                stdout.Should().Contain($"[DoggyDog] Process {attachedProcess.Id} exited cleanly (code {attachedProcess.ExitCode}). No recovery needed.");
            }

            public static async Task ShouldHaveNotFoundProcess(string stdout, int pid)
            {
                stdout.Should().Contain($"[DoggyDog] No process with PID {pid} found. Exiting.");
            }
        }
    }
}

