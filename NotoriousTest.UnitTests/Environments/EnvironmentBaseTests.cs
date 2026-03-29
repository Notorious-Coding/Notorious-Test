using AwesomeAssertions;

using FakeItEasy;

using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core.Exceptions;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.UnitTests.Stubs;

using System.Reflection;

namespace NotoriousTest.UnitTests.Environments;

public class EnvironmentBaseTests
{
    private readonly ITestLogger _testLogger;
    private readonly IRegistry _registry;
    private readonly IWatchDog _watchDog;

    public EnvironmentBaseTests()
    {
        _testLogger = A.Fake<ITestLogger>();
        _registry = A.Fake<IRegistry>();
        _watchDog = A.Fake<IWatchDog>();
    }

    [Fact]
    public async Task Initialize_BuildsServiceProvider()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        await stub.Initialize();
        stub.PublicServiceProvider.Should().NotBeNull();
        stub.PublicServiceProvider!.GetService<ITestLogger>().Should().NotBeNull().And.Be(_testLogger);
        stub.PublicServiceProvider!.GetService<IRegistry>().Should().NotBeNull().And.Be(_registry);
        stub.PublicServiceProvider!.GetService<IWatchDog>().Should().NotBeNull().And.Be(_watchDog);
    }

    [Fact]
    public async Task Initialize_CallsConfigureEnvironment()
    {
        bool called = false;
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry)
        {
            OnConfigureEnvironment = () =>
            {
                called = true;
                return Task.CompletedTask;
            }
        };

        await stub.Initialize();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Initialize_CallsSetupRegistry()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        await stub.Initialize();

        A.CallTo(() => _registry.Ensure()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Initialize_StartsWatchdog()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        await stub.Initialize();
        A.CallTo(() => _watchDog.Start(A<Assembly>._, A<int>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Initialize_InitializesInfrastructuresInAscendingOrder()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => initializationOrder.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => initializationOrder.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => initializationOrder.Add(3) });

        await stub.Initialize();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task AddInfrastructure_Generic_SetsContextId()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);

        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure<InfrastructureStub>();
        };

        await stub.Initialize();
        InfrastructureStub infrastructure = stub.GetInfrastructure<InfrastructureStub>();
        infrastructure.ContextId.Should().Be(stub.EnvironmentId);
    }

    [Fact]
    public void AddInfrastructure_Instance_SetsContextId()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        InfrastructureStub infra = new(_testLogger, _registry);

        stub.AddInfrastructure(infra);
        infra.ContextId.Should().Be(stub.EnvironmentId);
    }

    [Fact]
    public void GetInfrastructure_ExistingType_ReturnsInstance()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        InfrastructureStub infra = new(_testLogger, _registry);
        stub.AddInfrastructure(infra);
        InfrastructureStub retrieved = stub.GetInfrastructure<InfrastructureStub>();
        retrieved.Should().BeSameAs(infra);
    }

    [Fact]
    public void GetInfrastructure_MissingType_ThrowsInfrastructureNotFoundException()
    {
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        Action act = () => stub.GetInfrastructure<InfrastructureStub>();
        act.Should().Throw<InfrastructureNotFoundException>();
    }

    [Fact]
    public async Task Reset_OnlyResetsInfrastructuresWithAutoResetEnabled()
    {
        List<int> resetOrder = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2, autoReset: true) { OnReset = async () => resetOrder.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1, autoReset: false) { OnReset = async () => resetOrder.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3, autoReset: true) { OnReset = async () => resetOrder.Add(3) });
        await stub.Reset();
        resetOrder.Should().Equal(2, 3);
    }

    [Fact]
    public async Task Reset_ResetsInfrastructuresInAscendingOrder()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnReset = async () => initializationOrder.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnReset = async () => initializationOrder.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnReset = async () => initializationOrder.Add(3) });

        await stub.Reset();
        initializationOrder.Should().Equal(1, 2, 3);

    }

    [Fact]
    public async Task Destroy_DestroysAllInfrastructures()
    {
        List<int> destroyOrder = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnDestroy = async () => destroyOrder.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnDestroy = async () => destroyOrder.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnDestroy = async () => destroyOrder.Add(3) });
        await stub.Destroy();
        destroyOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void EnvironmentId_IsUniquePerInstance()
    {
        EnvironmentStub stub1 = new(_testLogger, _watchDog, _registry);
        EnvironmentStub stub2 = new(_testLogger, _watchDog, _registry);
        stub1.EnvironmentId.Should().NotBeNull();
        stub1.EnvironmentId.Value.Should().NotBeEmpty();
        stub2.EnvironmentId.Should().NotBeNull();
        stub2.EnvironmentId.Value.Should().NotBeEmpty();

        stub1.EnvironmentId.Should().NotBe(stub2.EnvironmentId);
    }

    [Fact]
    public async Task Initialize_OrderNull_InfrastructuresInitializedInAddOrder()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(3) });
        await stub.Initialize();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Initialize_MixedOrders_InitializedInAscendingOrder()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => initializationOrder.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => initializationOrder.Add(3) });
        await stub.Initialize();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Reset_SameOrderingAsInitialize()
    {
        List<int> order = new();
        EnvironmentStub stub = new(_testLogger, _watchDog, _registry);
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => order.Add(2), OnReset = async () => order.Add(2) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => order.Add(1), OnReset = async () => order.Add(1) });
        stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => order.Add(3), OnReset = async () => order.Add(3) });
        await stub.Initialize();
        await stub.Reset();
        // The reset should be in the same order as initialize
        order.Should().Equal(1, 2, 3, 1, 2, 3);
    }
}
