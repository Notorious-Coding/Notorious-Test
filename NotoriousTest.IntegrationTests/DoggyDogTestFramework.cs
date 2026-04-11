using AwesomeAssertions;

using Dapper;

using NotoriousTest.Core;
using NotoriousTest.Core.Infrastructures;
using NotoriousTest.Core.Infrastructures.Cleaner;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Sqlite;
using NotoriousTest.SqlLiteRegistry;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;


namespace NotoriousTest.IntegrationTests
{
    public static class DoggyDogTestFramework
    {
        public static class Arrange
        {
            public static Process? StartFakeProcess(Guid environmentId, int exitCode = 0, int timeToExit = 5)
            {
                ProcessStartInfo startInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c timeout /t {timeToExit} && exit {exitCode}",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                    }
                    : new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = $"-c \"sleep {timeToExit} && exit {exitCode}\"",
                        UseShellExecute = true,
                        CreateNoWindow = false,
                    };

                Process? fakeProcess = Process.Start(startInfo);
                if (exitCode == 0 && fakeProcess != null)
                {
                    File.WriteAllText(Path.Combine(Path.GetTempPath(), $"nt-{environmentId}.signal"), "OK");
                }

                return fakeProcess;
            }

            public static async Task CreateRegistry(SqliteInfrastructure registryInfrastructure)
            {
                using var connection = registryInfrastructure.GetDatabaseConnection();
                await connection.ExecuteAsync(SqliteRegistryProvider.ENSURE_REGISTRY);

            }
            public static async Task<InfrastuctureRegistryEntry> PopulateRegistryWithFakeInfrastructure(SqliteInfrastructure registryInfrastructure, Process attachedProcess, Type fakeInfrastructureType, Guid environmentId)
            {
                using var connection = registryInfrastructure.GetDatabaseConnection();

                var entry = new InfrastuctureRegistryEntry
                {
                    InfrastructureId = Guid.NewGuid(),
                    EnvironmentId = environmentId,
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
                public Task CleanAfterCrash(EnvironmentId contextId, Guid infrastructureId, object? metadata = null)
                {
                    Console.WriteLine(Message, metadata, contextId.Value, infrastructureId);
                    return Task.CompletedTask;
                }
            }
            [Cleaner(typeof(FakeCleaner))]
            public class FakeInfrastructureWithCleaner : Infrastructure<string>
            {
                public FakeInfrastructureWithCleaner(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
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
            }

            [Cleaner(typeof(FakeCleaner))]
            public class FakeInfrastructure2WithCleaner : Infrastructure<string>
            {
                public FakeInfrastructure2WithCleaner(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
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
            }

            public class FakeInfrastructureWithoutCleanerAttribute : Infrastructure<string>
            {
                public FakeInfrastructureWithoutCleanerAttribute(EnvironmentId contextId, ITestLogger logger, IRegistry provider) : base(contextId, logger, provider)
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
            }
        }

        public static class Act
        {
            public static (Process? Process, StringBuilder StdoutBuilder) StartDoggyDog(int processId, string assembly, string connectionString, Guid environmentId)
            {
                var doggyDogPath = Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "DoggyDog.exe" : "DoggyDog");
                Process? doggyDogProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = doggyDogPath,
                    Arguments = $"--pid {processId} --assembly \"{assembly}\" --connectionString \"{connectionString}\" --environment {environmentId}",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                });

                doggyDogProcess!.StandardInput.Close();
                var stdoutBuilder = new StringBuilder();

                doggyDogProcess.OutputDataReceived += (_, e) =>
                {
                    if (e.Data is not null) stdoutBuilder.AppendLine(e.Data);
                };
                doggyDogProcess.BeginOutputReadLine(); // consomme le buffer de façon async
                return (doggyDogProcess, stdoutBuilder);
            }
        }

        public static class Assert
        {
            public static async Task ShouldHaveFoundInfrastructureToClean(string stdout, int count, Guid environmentId)
            {
                stdout.Should().Contain($"[DoggyDog] {count} infrastructure(s) registered for EID {environmentId}. Starting cleanup...");
            }
            public static async Task ShouldHaveNotFoundInfrastructureToClean(string stdout, Guid environmentId)
            {
                stdout.Should().Contain($"[DoggyDog] No registered infrastructure found for EID {environmentId}. Nothing to clean up.");
            }
            public static async Task ShouldHaveCleanedEntry(string stdout, SqliteInfrastructure registry, InfrastuctureRegistryEntry entry)
            {
                using var connection = registry.GetDatabaseConnection();
                var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM InfrastructureRegistry WHERE EnvironmentId = @EnvironmentId", new { EnvironmentId = entry.EnvironmentId });

                count.Should().Be(0);

                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleanup in progress...");
                stdout.Should().Contain($"[{entry.InfrastructureType.Name}] Cleaning using {nameof(Arrange.FakeCleaner)}");
                stdout.Should().Contain(string.Format(Arrange.FakeCleaner.Message, entry.Metadata, entry.EnvironmentId, entry.InfrastructureId));
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

            public static async Task ShouldHaveInitiatedRecovery(string stdout, Process? attachedProcess, Guid environmentId)
            {
                stdout.Should().Contain($"[DoggyDog] Process {attachedProcess.Id} for EID {environmentId} exited abnormally. Initiating crash recovery...");
            }

            public static async Task ShouldHaveMonitoredProcess(string stdout, Process? attachedProcess)
            {
                stdout.Should().Contain($"[DoggyDog] Monitoring PID {attachedProcess.Id} - awaiting termination...");
            }

            public static async Task ShouldHaveReceivedExitSignal(string stdout, Guid environmentId)
            {
                stdout.Should().Contain($"[DoggyDog] Success signal found for {environmentId}.");
                File.Exists(Path.Combine(Path.GetTempPath(), $"nt-{environmentId}.signal")).Should().BeFalse();
            }

            public static async Task ShouldHaveFoundRegistryFile(string stdout, string registryPath)
            {
                stdout.Should().Contain($"[DoggyDog] Using registry file at {registryPath}");
            }

            public static async Task ShouldHaveNotFoundRegistryFile(string stdout, string registryPath)
            {
                stdout.Should().Contain($"[DoggyDog] Registry file not found at {registryPath}");
            }

            public static async Task ShouldHaveExitNormally(string stdout, Process? attachedProcess, Guid environmentId)
            {
                stdout.Should().Contain($"[DoggyDog] Process {attachedProcess.Id} for EID {environmentId} exited cleanly. No recovery needed.");
            }

            public static async Task ShouldHaveNotFoundProcess(string stdout, int pid)
            {
                stdout.Should().Contain($"[DoggyDog] No process with PID {pid} found. Exiting.");
            }
        }
    }
}

