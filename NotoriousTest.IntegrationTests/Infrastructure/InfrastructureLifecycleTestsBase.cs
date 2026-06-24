using FakeItEasy;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.IntegrationTests.SystemUnderTest;
using NotoriousTest.SqlLiteRegistry;
using NotoriousTest.XUnit;

namespace NotoriousTest.IntegrationTests.Infrastructure;

public class InfrastructureLifecycleTestsBase(XUnitFixture<NotoriousTestEnvironment> environment)
    : IntegrationTest<NotoriousTestEnvironment>(environment)
{
    [Fact]
    public async Task Initialize_Should_RegisterInfrastructureInRegistry()
    {
        EnvironmentId environmentId = Guid.NewGuid();
        NotoriousTestRegistryInfrastructure notoriousTestRegistryInfra =
            CurrentEnvironment.GetInfrastructure<NotoriousTestRegistryInfrastructure>();
        var registry = new SqliteRegistryRepository(new SqliteRegistryRepositoryConfiguration
        {
            ConnectionString = notoriousTestRegistryInfra.GetDatabaseConnectionString()
        });
        await using var infrastructure =
            new TestFramework.Arrange.FakeInfrastructure(environmentId, A.Fake<ITestLogger>(), registry);

        await infrastructure.InitializeAsync();

        await TestFramework.Assert.InfrastructureShouldBeRegistered(notoriousTestRegistryInfra, infrastructure.Id);
    }

    [Fact]
    public async Task Reset_Should_UpdateLastResetDateInRegistry()
    {
        EnvironmentId environmentId = Guid.NewGuid();
        NotoriousTestRegistryInfrastructure notoriousTestRegistryInfra =
            CurrentEnvironment.GetInfrastructure<NotoriousTestRegistryInfrastructure>();
        var registry = new SqliteRegistryRepository(new SqliteRegistryRepositoryConfiguration
        {
            ConnectionString = notoriousTestRegistryInfra.GetDatabaseConnectionString()
        });
        await using var infrastructure =
            new TestFramework.Arrange.FakeInfrastructure(environmentId, A.Fake<ITestLogger>(), registry);

        await infrastructure.InitializeAsync();
        await infrastructure.ResetAsync();

        await TestFramework.Assert.InfrastructureShouldHaveBeenReset(notoriousTestRegistryInfra, infrastructure.Id);
    }

    [Fact]
    public async Task Destroy_Should_RemoveInfrastructureFromRegistry()
    {
        EnvironmentId environmentId = Guid.NewGuid();
        NotoriousTestRegistryInfrastructure notoriousTestRegistryInfra =
            CurrentEnvironment.GetInfrastructure<NotoriousTestRegistryInfrastructure>();
        var registry = new SqliteRegistryRepository(new SqliteRegistryRepositoryConfiguration
        {
            ConnectionString = notoriousTestRegistryInfra.GetDatabaseConnectionString()
        });
        var infrastructure =
            new TestFramework.Arrange.FakeInfrastructure(environmentId, A.Fake<ITestLogger>(), registry);

        await infrastructure.InitializeAsync();
        await infrastructure.DestroyAsync();

        await TestFramework.Assert.InfrastructureShouldBeRemovedFromRegistry(notoriousTestRegistryInfra,
            infrastructure.Id);
    }
}
