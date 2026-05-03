using AwesomeAssertions;

using NotoriousTest.Sqlite;
using NotoriousTest.XUnit;

using System.Diagnostics;
using System.Text;
using NotoriousTest.Core.Registry;
using NotoriousTest.IntegrationTests.SystemUnderTest;


namespace NotoriousTest.IntegrationTests.DoggyDog
{
    public class DoggyDogTestsBase(XUnitFixture<NotoriousTestEnvironment> environment)
        : IntegrationTest<NotoriousTestEnvironment>(environment)
    {
        private const int FAKE_PROCESS_WAIT_TIME = 1;

        [Fact]
        public async Task DoggyDog_Should_Do_Nothing_When_Process_Exit_Normally()
        {
            Guid environmentId = Guid.NewGuid();
            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, timeToExit: FAKE_PROCESS_WAIT_TIME);

            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveReceivedExitSignal(stdout, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveExitNormally(stdout, process, environmentId);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Cleanup_Orphan_Infrastructures_When_Process_Exit_Abnormally()
        {
            var environmentId = Guid.NewGuid();
            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            Process process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            InfrastuctureRegistryEntry fakeEntry = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), environmentId);

            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundInfrastructureToClean(stdout, count: 1, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveCleanedEntry(stdout, registry, fakeEntry);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Exit_When_Monitored_Process_Not_Found()
        {
            var environmentId = Guid.NewGuid();
            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            const int PROCESS_ID = 999999;
            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(PROCESS_ID, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundProcess(stdout, PROCESS_ID);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Exit_When_Registry_File_Not_Found()
        {
            var environmentId = Guid.NewGuid();

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            const string FAKE_REGISTRY_PATH = "C:\\fake\\path\\registry.db";
            const string FAKE_CONNECTION_STRING = $"DataSource={FAKE_REGISTRY_PATH}";
            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTestsBase).Assembly.Location, FAKE_CONNECTION_STRING, environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            string stdout = stdoutBuilder.ToString();

            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundRegistryFile(stdout, FAKE_REGISTRY_PATH);
            doggyDogProcess.ExitCode.Should().Be(1);
        }

        [Fact]
        public async Task DoggyDog_Should_Do_Nothing_When_Process_Crashes_With_No_Registered_Infrastructures()
        {
            var environmentId = Guid.NewGuid();

            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            Process process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundInfrastructureToClean(stdout, environmentId);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Skip_Infrastructure_Without_Cleaner_Attribute_When_Process_Crashes()
        {
            var environmentId = Guid.NewGuid();
            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            InfrastuctureRegistryEntry fakeEntry = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithoutCleanerAttribute), environmentId);

            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundInfrastructureToClean(stdout, count: 1, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundCleanerAttribute(stdout, registry, fakeEntry);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Cleanup_Multiple_Orphan_Infrastructures_When_Process_Crashes()
        {
            var environmentId = Guid.NewGuid();
            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            Process fakeProcess = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            InfrastuctureRegistryEntry fakeEntry1 = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, fakeProcess, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), environmentId);
            InfrastuctureRegistryEntry fakeEntry2 = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, fakeProcess, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructure2WithCleaner), environmentId);

            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(fakeProcess.Id, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, fakeProcess);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, fakeProcess, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundInfrastructureToClean(stdout, count: 2, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveCleanedEntry(stdout, registry, fakeEntry1);
            await DoggyDogTestFramework.Assert.ShouldHaveCleanedEntry(stdout, registry, fakeEntry2);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Display_InfrastructureLifecycleEvents()
        {
            var environmentId = Guid.NewGuid();
            SqliteInfrastructure registry = Environment.GetInfrastructure<SqliteInfrastructure>();

            Process fakeProcess = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, timeToExit: 999);
            (Process doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(fakeProcess.Id, typeof(DoggyDogTestsBase).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);
            await Task.Delay(1000, TestContext.Current.CancellationToken);

            InfrastuctureRegistryEntry fakeEntry1 = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, fakeProcess, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveCatchInitEvent(stdoutBuilder, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), TestContext.Current.CancellationToken);

            await DoggyDogTestFramework.Arrange.TriggerInfrastructureReset(registry, fakeEntry1.InfrastructureId);
            await DoggyDogTestFramework.Assert.ShouldHaveCatchResetEvent(stdoutBuilder, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), TestContext.Current.CancellationToken);

            await DoggyDogTestFramework.Arrange.DestroyInfrastructureFromRegistry(registry, fakeEntry1.InfrastructureId);
            await DoggyDogTestFramework.Assert.ShouldHaveCatchDestroyEvent(stdoutBuilder, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), TestContext.Current.CancellationToken);

            fakeProcess.Kill(true);
            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            string stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, fakeProcess);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
        }
    }
}
