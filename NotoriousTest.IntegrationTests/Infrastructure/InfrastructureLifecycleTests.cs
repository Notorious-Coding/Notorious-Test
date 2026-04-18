using FakeItEasy;

using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.SqlLiteRegistry;
using NotoriousTest.XUnit;

namespace NotoriousTest.IntegrationTests.Infrastructure;

public class InfrastructureLifecycleTests : IntegrationTest<InfrastructureLifecycleEnvironment>
{
    public InfrastructureLifecycleTests(InfrastructureLifecycleEnvironment environment) : base(environment) { }

    [Fact]
    public async Task Initialize_Should_RegisterInfrastructureInRegistry()
    {
        EnvironmentId environmentId = Guid.NewGuid();
        var registryInfra = CurrentEnvironment.GetInfrastructure<RegistryDatabaseInfrastructure>();
        await using var registry = new SqliteRegistryProvider(new SqliteRegistryProviderConfiguration
        {
            ConnectionString = registryInfra.GetDatabaseConnectionString()
        });
        await using var infrastructure = new TestFramework.Arrange.FakeInfrastructure(environmentId, A.Fake<ITestLogger>(), registry);

        await infrastructure.InitializeAsync();

        await TestFramework.Assert.InfrastructureShouldBeRegistered(registryInfra, infrastructure.Id);
    }

    [Fact]
    public async Task Reset_Should_UpdateLastResetDateInRegistry()
    {
        EnvironmentId environmentId = Guid.NewGuid();
        var registryInfra = CurrentEnvironment.GetInfrastructure<RegistryDatabaseInfrastructure>();
        await using var registry = new SqliteRegistryProvider(new SqliteRegistryProviderConfiguration
        {
            ConnectionString = registryInfra.GetDatabaseConnectionString()
        });
        await using var infrastructure = new TestFramework.Arrange.FakeInfrastructure(environmentId, A.Fake<ITestLogger>(), registry);

        await infrastructure.InitializeAsync();
        await infrastructure.ResetAsync();

        await TestFramework.Assert.InfrastructureShouldHaveBeenReset(registryInfra, infrastructure.Id);
    }

    [Fact]
    public async Task Destroy_Should_RemoveInfrastructureFromRegistry()
    {
        EnvironmentId environmentId = Guid.NewGuid();
        var registryInfra = CurrentEnvironment.GetInfrastructure<RegistryDatabaseInfrastructure>();
        await using var registry = new SqliteRegistryProvider(new SqliteRegistryProviderConfiguration
        {
            ConnectionString = registryInfra.GetDatabaseConnectionString()
        });
        var infrastructure = new TestFramework.Arrange.FakeInfrastructure(environmentId, A.Fake<ITestLogger>(), registry);

        await infrastructure.InitializeAsync();
        await infrastructure.DestroyAsync();

        await TestFramework.Assert.InfrastructureShouldBeRemovedFromRegistry(registryInfra, infrastructure.Id);
    }
}
