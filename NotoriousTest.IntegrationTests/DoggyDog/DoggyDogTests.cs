using AwesomeAssertions;

using NotoriousTest.Sqlite;
using NotoriousTest.XUnit;

using System.Diagnostics;
using System.Text;



namespace NotoriousTest.IntegrationTests.DoggyDog
{
    public class DoggyDogTests : IntegrationTest<DoggyDogEnvironment>
    {
        private const int FAKE_PROCESS_WAIT_TIME = 1;
        public DoggyDogTests(DoggyDogEnvironment environment) : base(environment)
        {
        }

        [Fact]
        public async Task DoggyDog_Should_Do_Nothing_When_Process_Exit_Normally()
        {
            Guid environmentId = Guid.NewGuid();
            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }

            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();

            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            var stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveReceivedExitSignal(stdout, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveExitNormally(stdout, process, environmentId);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Cleanup_Orphan_Infrastructures_When_Process_Exit_Abnormally()
        {
            Guid environmentId = Guid.NewGuid();
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }
            var fakeEntry = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), environmentId);

            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            var stdout = stdoutBuilder.ToString();
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
            Guid environmentId = Guid.NewGuid();
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            const int PROCESS_ID = 12345;
            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(PROCESS_ID, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            var stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundProcess(stdout, PROCESS_ID);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Exit_When_Registry_File_Not_Found()
        {
            Guid environmentId = Guid.NewGuid();

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            const string FAKE_REGISTRY_PATH = "C:\\fake\\path\\registry.db";
            const string FAKE_CONNECTION_STRING = $"DataSource={FAKE_REGISTRY_PATH}";
            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, FAKE_CONNECTION_STRING, environmentId);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            var stdout = stdoutBuilder.ToString();

            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundRegistryFile(stdout, FAKE_REGISTRY_PATH);
            doggyDogProcess.ExitCode.Should().Be(1);
        }

        [Fact]
        public async Task DoggyDog_Should_Do_Nothing_When_Process_Crashes_With_No_Registered_Infrastructures()
        {
            Guid environmentId = Guid.NewGuid();

            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }

            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            var stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process, environmentId);
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundInfrastructureToClean(stdout, environmentId);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Skip_Infrastructure_Without_Cleaner_Attribute_When_Process_Crashes()
        {
            Guid environmentId = Guid.NewGuid();
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }
            var fakeEntry = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithoutCleanerAttribute), environmentId);

            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            var stdout = stdoutBuilder.ToString();
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
            Guid environmentId = Guid.NewGuid();
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? fakeProcess = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, exitCode: 1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (fakeProcess == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }
            var fakeEntry1 = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, fakeProcess, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), environmentId);
            var fakeEntry2 = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, fakeProcess, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructure2WithCleaner), environmentId);

            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(fakeProcess.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            var stdout = stdoutBuilder.ToString();
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
            Guid environmentId = Guid.NewGuid();
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? fakeProcess = DoggyDogTestFramework.Arrange.StartFakeProcess(environmentId, timeToExit: 999);
            (Process? doggyDogProcess, StringBuilder stdoutBuilder) = DoggyDogTestFramework.Act.StartDoggyDog(fakeProcess.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString(), environmentId);

            await Task.Delay(500, TestContext.Current.CancellationToken);
            var fakeEntry1 = await DoggyDogTestFramework.Arrange.CreateInfrastructureInRegistry(registry, fakeProcess, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner), environmentId);
            await Task.Delay(500, TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Arrange.TriggerInfrastructureReset(registry, fakeEntry1.InfrastructureId);
            await Task.Delay(500, TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Arrange.DestroyInfrastructureFromRegistry(registry, fakeEntry1.InfrastructureId);


            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            var stdout = stdoutBuilder.ToString();
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, fakeProcess);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveCatchInitEvent(stdout, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner));
            await DoggyDogTestFramework.Assert.ShouldHaveCatchResetEvent(stdout, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner));
            await DoggyDogTestFramework.Assert.ShouldHaveCatchDestroyEvent(stdout, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner));
        }
    }
}
