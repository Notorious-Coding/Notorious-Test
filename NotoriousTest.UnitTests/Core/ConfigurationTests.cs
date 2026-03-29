using AwesomeAssertions;

using FakeItEasy;

using NotoriousTest.Core.Configuration;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.UnitTests.Stubs;

namespace NotoriousTest.UnitTests.Core;

public class ConfigurationTests
{

    private readonly ITestLogger _testLogger;
    private readonly IRegistry _registry;
    private readonly IWatchDog _watchDog;

    public ConfigurationTests()
    {
        _testLogger = A.Fake<ITestLogger>();
        _registry = A.Fake<IRegistry>();
        _watchDog = A.Fake<IWatchDog>();
    }

    [Fact]
    public async Task AggregateConfiguration_MultipleProducers_MergesAllEntries()
    {
        var infrastructure1 = new InfrastructureStub(_testLogger, _registry);
        var infrastructure2 = new InfrastructureStub(_testLogger, _registry);
        var infrastructure3 = new ConsumerInfrastructureStub(_testLogger, _registry);
        infrastructure1.AddEntry("key", "value");
        infrastructure2.AddEntry("key2", "value2");

        EnvironmentStub environment = new(_testLogger, _watchDog, _registry);

        environment.AddInfrastructure(infrastructure1);
        environment.AddInfrastructure(infrastructure2);
        environment.AddInfrastructure(infrastructure3);

        await environment.Initialize();

        infrastructure3.ConsumedConfiguration.Should().HaveCount(2)
            .And.Contain(entry => entry.Key == "key" && entry.Value.Equals("value"))
            .And.Contain(entry => entry.Key == "key2" && entry.Value.Equals("value2"));
    }

    [Fact]
    public async Task AggregateConfiguration_NoConfigurationProduced_ReturnsEmptyList()
    {
        var infrastructure1 = new InfrastructureStub(_testLogger, _registry);
        var infrastructure2 = new ConsumerInfrastructureStub(_testLogger, _registry);

        EnvironmentStub environment = new(_testLogger, _watchDog, _registry);


        environment.AddInfrastructure(infrastructure1);
        environment.AddInfrastructure(infrastructure2);

        await environment.Initialize();

        infrastructure2.ConsumedConfiguration.Should().BeEmpty();
    }

    [Fact]
    public async Task ConfigurationConsumer_ReceivesConfigurationBeforeItsOwnInitialization()
    {
        var infrastructure1 = new InfrastructureStub(_testLogger, _registry);
        var infrastructure2 = new ConsumerInfrastructureStub(_testLogger, _registry);

        EnvironmentStub environment = new(_testLogger, _watchDog, _registry);

        infrastructure1.AddEntry("key", "value");

        infrastructure2.OnInitialize = async () =>
        {
            // During initialization, the consumer should already have access to the configuration produced by infrastructure1
            infrastructure2.ConsumedConfiguration.Should().Contain(entry => entry.Key == "key" && entry.Value.Equals("value"));
        };
        environment.AddInfrastructure(infrastructure1);
        environment.AddInfrastructure(infrastructure2);

        await environment.Initialize();

        infrastructure2.ConsumedConfiguration.Should().ContainSingle().Which.Should().BeEquivalentTo(new ConfigurationEntry<string>("value", "key"));
    }

}
