using AwesomeAssertions;

using NotoriousTest.PostgreSql;
using NotoriousTest.XUnit;

using System.Diagnostics;



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
            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }

            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();

            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString());

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveExitNormally(stdout, process);
            doggyDogProcess.ExitCode.Should().Be(0);
        }



        [Fact]
        public async Task DoggyDog_Should_Cleanup_Orphan_Infrastructures_When_Process_Exit_Abnormally()
        {

            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(exitCode: -1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }
            var fakeEntry = await DoggyDogTestFramework.Arrange.PopulateRegistryWithFakeInfrastructure(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner));

            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString());

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundInfrastructureToClean(stdout, count: 1, process.Id);
            await DoggyDogTestFramework.Assert.ShouldHaveCleanedEntry(stdout, registry, fakeEntry);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Exit_When_Monitored_Process_Not_Found()
        {
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            const int PROCESS_ID = 12345;
            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(PROCESS_ID, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString());

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundProcess(stdout, PROCESS_ID);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Exit_When_Registry_File_Not_Found()
        {

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(exitCode: -1, timeToExit: FAKE_PROCESS_WAIT_TIME);
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            const string FAKE_REGISTRY_PATH = "C:\\fake\\path\\registry.db";
            const string FAKE_CONNECTION_STRING = $"DataSource={FAKE_REGISTRY_PATH}";
            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, FAKE_CONNECTION_STRING);

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);
            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);

            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundRegistryFile(stdout, FAKE_REGISTRY_PATH);
            doggyDogProcess.ExitCode.Should().Be(-1);
        }

        [Fact]
        public async Task DoggyDog_Should_Do_Nothing_When_Process_Crashes_With_No_Registered_Infrastructures()
        {

            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(-1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }

            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString());

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundInfrastructureToClean(stdout, process.Id);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Skip_Infrastructure_Without_Cleaner_Attribute_When_Process_Crashes()
        {
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(-1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }
            var fakeEntry = await DoggyDogTestFramework.Arrange.PopulateRegistryWithFakeInfrastructure(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithoutCleanerAttribute));

            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString());

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundInfrastructureToClean(stdout, 1, process.Id);
            await DoggyDogTestFramework.Assert.ShouldHaveNotFoundCleanerAttribute(stdout, registry, fakeEntry);
            doggyDogProcess.ExitCode.Should().Be(0);
        }

        [Fact]
        public async Task DoggyDog_Should_Cleanup_Multiple_Orphan_Infrastructures_When_Process_Crashes()
        {
            var registry = CurrentEnvironment.GetInfrastructure<SqliteInfrastructure>();
            await DoggyDogTestFramework.Arrange.CreateRegistry(registry);

            Process? process = DoggyDogTestFramework.Arrange.StartFakeProcess(-1, timeToExit: FAKE_PROCESS_WAIT_TIME);

            if (process == null)
            {
                Assert.Fail("Fail to launch cmd.exe");
            }
            var fakeEntry1 = await DoggyDogTestFramework.Arrange.PopulateRegistryWithFakeInfrastructure(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructureWithCleaner));
            var fakeEntry2 = await DoggyDogTestFramework.Arrange.PopulateRegistryWithFakeInfrastructure(registry, process, typeof(DoggyDogTestFramework.Arrange.FakeInfrastructure2WithCleaner));

            Process? doggyDogProcess = DoggyDogTestFramework.Act.StartDoggyDog(process.Id, typeof(DoggyDogTests).Assembly.Location, registry.GetDatabaseConnectionString());

            if (doggyDogProcess == null)
            {
                Assert.Fail("Fail to launch DoggyDog.exe");
            }

            await doggyDogProcess.WaitForExitAsync(TestContext.Current.CancellationToken);

            string stdout = await doggyDogProcess.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
            await DoggyDogTestFramework.Assert.ShouldHaveMonitoredProcess(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundRegistryFile(stdout, registry.GetPath());
            await DoggyDogTestFramework.Assert.ShouldHaveInitiatedRecovery(stdout, process);
            await DoggyDogTestFramework.Assert.ShouldHaveFoundInfrastructureToClean(stdout, count: 2, process.Id);
            await DoggyDogTestFramework.Assert.ShouldHaveCleanedEntry(stdout, registry, fakeEntry1);
            await DoggyDogTestFramework.Assert.ShouldHaveCleanedEntry(stdout, registry, fakeEntry2);
            doggyDogProcess.ExitCode.Should().Be(0);
        }
    }
}
