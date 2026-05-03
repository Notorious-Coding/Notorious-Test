using AwesomeAssertions;

using FakeItEasy;

using Microsoft.Extensions.DependencyInjection;

using NotoriousTest.Core;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Exceptions;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.UnitTests.Stubs;

using System.Reflection;

namespace NotoriousTest.UnitTests.Environments;

public class EnvironmentBaseTests
{
    private readonly ITestLogger _testLogger;
    private readonly IRegistry _registry;
    private readonly IWatchDog _watchDog;
    private readonly IRuntime _runtime;
    private readonly IServiceProvider _provider;
    private readonly EnvironmentId _environmentId = EnvironmentId.Create();

    public EnvironmentBaseTests()
    {
        _testLogger = A.Fake<ITestLogger>();
        _registry = A.Fake<IRegistry>();
        _watchDog = A.Fake<IWatchDog>();
        _runtime = A.Fake<IRuntime>();
        _provider = new ServiceCollection()
            .AddSingleton(_testLogger)
            .AddSingleton(_registry)
            .AddSingleton(_watchDog)
            .AddSingleton(_runtime)
            .AddSingleton(_environmentId)
            .BuildServiceProvider();
    }


    [Fact]
    public async Task Initialize_Should_CallConfigureEnvironment()
    {
        bool called = false;
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider)
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
    public async Task Initialize_Should_CallSetupRegistry()
    {
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        await stub.Initialize();

        A.CallTo(() => _registry.Ensure()).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Initialize_Should_StartWatchdog()
    {
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        await stub.Initialize();
        A.CallTo(() => _watchDog.Start(A<Assembly>._, A<int>._, A<EnvironmentId>._, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Initialize_Should_InitializeInfrastructuresInAscendingOrder()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => initializationOrder.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => initializationOrder.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => initializationOrder.Add(3) });
        };
        await stub.Initialize();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task AddInfrastructure_Should_SetContextId_WhenGeneric()
    {

        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);

        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure<InfrastructureStub>();
        };

        await stub.Initialize();
        InfrastructureStub infrastructure = stub.GetInfrastructure<InfrastructureStub>();
        infrastructure.EnvironmentId.Should().Be(stub.EnvironmentId);
    }

    [Fact]
    public async Task AddInfrastructure_Should_SetContextId_WhenInstance()
    {
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        InfrastructureStub infra = new(_testLogger, _registry);

        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(infra);
        };

        await stub.Initialize();
        infra.EnvironmentId.Should().Be(stub.EnvironmentId);
    }

    [Fact]
    public async Task GetInfrastructure_Should_ReturnInstance_WhenTypeExists()
    {
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        InfrastructureStub infra = new(_testLogger, _registry);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(infra);
        };

        await stub.Initialize();
        InfrastructureStub retrieved = stub.GetInfrastructure<InfrastructureStub>();
        retrieved.Should().BeSameAs(infra);
    }

    [Fact]
    public void GetInfrastructure_Should_ThrowInfrastructureNotFoundException_WhenTypeIsMissing()
    {
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        Action act = () => stub.GetInfrastructure<InfrastructureStub>();
        act.Should().Throw<InfrastructureNotFoundException>();
    }

    [Fact]
    public async Task Reset_Should_OnlyResetInfrastructuresWithAutoResetEnabled()
    {
        List<int> resetOrder = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);

        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2, autoReset: true) { OnReset = async () => resetOrder.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1, autoReset: false) { OnReset = async () => resetOrder.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3, autoReset: true) { OnReset = async () => resetOrder.Add(3) });
        };

        await stub.Initialize();
        await stub.Reset();
        resetOrder.Should().Equal(2, 3);
    }

    [Fact]
    public async Task Reset_Should_ResetInfrastructuresInAscendingOrder()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);

        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnReset = async () => initializationOrder.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnReset = async () => initializationOrder.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnReset = async () => initializationOrder.Add(3) });
        };
        await stub.Initialize();
        await stub.Reset();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Destroy_Should_DestroyAllInfrastructures()
    {
        List<int> destroyOrder = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnDestroy = async () => destroyOrder.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnDestroy = async () => destroyOrder.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnDestroy = async () => destroyOrder.Add(3) });
        };
        await stub.Initialize();
        await stub.Destroy();
        destroyOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void EnvironmentId_Should_BeUniquePerInstance()
    {
        EnvironmentStub stub1 = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        EnvironmentStub stub2 = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub1.EnvironmentId.Should().NotBeNull();
        stub1.EnvironmentId.Value.Should().NotBeEmpty();
        stub2.EnvironmentId.Should().NotBeNull();
        stub2.EnvironmentId.Value.Should().NotBeEmpty();

        stub1.EnvironmentId.Should().NotBe(stub2.EnvironmentId);
    }

    [Fact]
    public async Task Initialize_Should_InitializeInfrastructuresInAddOrder_WhenOrderIsNull()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(3) });
        };
        await stub.Initialize();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Initialize_Should_InitializeInAscendingOrder_WhenOrdersAreMixed()
    {
        List<int> initializationOrder = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => initializationOrder.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: null) { OnInitialize = async () => initializationOrder.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => initializationOrder.Add(3) });
        };
        await stub.Initialize();
        initializationOrder.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Reset_Should_UseTheSameOrderingAsInitialize()
    {
        List<int> order = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => order.Add(2), OnReset = async () => order.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => order.Add(1), OnReset = async () => order.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => order.Add(3), OnReset = async () => order.Add(3) });
        };

        await stub.Initialize();
        await stub.Reset();
        order.Should().Equal(1, 2, 3, 1, 2, 3);
    }

    [Fact]
    public async Task Initialize_Should_OrderMultipleInfrastructures()
    {
        List<int> order = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => order.Add(2), OnReset = async () => order.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => order.Add(2), OnReset = async () => order.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => order.Add(1), OnReset = async () => order.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => order.Add(1), OnReset = async () => order.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => order.Add(3), OnReset = async () => order.Add(3) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => order.Add(3), OnReset = async () => order.Add(3) });
        };
        await stub.Initialize();
        order.Should().Equal(1, 1, 2, 2, 3, 3);
    }

    [Fact]
    public async Task Initialize_Should_ExecuteIConsumerLast_InEachGroup()
    {
        List<int> order = new();
        EnvironmentStub stub = new(new EnvironmentSettings(), _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new ConsumerInfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => order.Add(22), OnReset = async () => order.Add(2) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 2) { OnInitialize = async () => order.Add(2), OnReset = async () => order.Add(2) });
            stub.AddInfrastructure(new ConsumerInfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => order.Add(11), OnReset = async () => order.Add(1) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 1) { OnInitialize = async () => order.Add(1), OnReset = async () => order.Add(1) });
            stub.AddInfrastructure(new ConsumerInfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => order.Add(33), OnReset = async () => order.Add(3) });
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry, order: 3) { OnInitialize = async () => order.Add(3), OnReset = async () => order.Add(3) });
        };

        await stub.Initialize();
        order.Should().Equal(1, 11, 2, 22, 3, 33);
    }

    // --- Watchdog disabled ---


    [Fact]
    public async Task Initialize_Should_NotSetupRegistry_WhenWatchdogIsDisabled()
    {
        EnvironmentSettings settings = new() { DisableWatchdog = true };
        EnvironmentStub stub =  new(settings, _watchDog, _registry, _runtime, _testLogger, _provider);

        await stub.Initialize();

        A.CallTo(() => _registry.Ensure()).MustNotHaveHappened();
    }

    [Fact]
    public async Task Initialize_Should_NotStartWatchdog_WhenWatchdogIsDisabled()
    {
        EnvironmentSettings settings = new() { DisableWatchdog = true };
        EnvironmentStub stub =  new(settings, _watchDog, _registry, _runtime, _testLogger, _provider);

        await stub.Initialize();

        A.CallTo(() => _watchDog.Start(A<Assembly>._, A<int>._, A<EnvironmentId>._, A<IEnumerable<string>>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Destroy_Should_NotSendSuccessSignal_WhenWatchdogIsDisabled()
    {
        EnvironmentSettings settings = new() { DisableWatchdog = true };
        EnvironmentStub stub =  new(settings, _watchDog, _registry, _runtime, _testLogger, _provider);

        await stub.Initialize();
        await stub.Destroy();

        A.CallTo(() => _watchDog.SendSuccessSignal(A<EnvironmentId>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Initialize_Should_NotRegisterInfrastructureInRegistry_WhenWatchdogIsDisabled()
    {
        EnvironmentSettings settings = new() { DisableWatchdog = true };
        EnvironmentStub stub =  new(settings, _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry));
        };

        await stub.Initialize();

        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Reset_Should_NotNotifyResetInRegistry_WhenWatchdogIsDisabled()
    {
        EnvironmentSettings settings = new() { DisableWatchdog = true };
        EnvironmentStub stub =  new(settings, _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry));
        };

        await stub.Initialize();
        await stub.Reset();

        A.CallTo(() => _registry.NotifyReset(A<Guid>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task Destroy_Should_NotRemoveInfrastructureFromRegistry_WhenWatchdogIsDisabled()
    {
        EnvironmentSettings settings = new() { DisableWatchdog = true };
        EnvironmentStub stub = new(settings, _watchDog, _registry, _runtime, _testLogger, _provider);
        stub.OnConfigureEnvironment = async () =>
        {
            stub.AddInfrastructure(new InfrastructureStub(_testLogger, _registry));
        };

        await stub.Initialize();
        await stub.Destroy();

        A.CallTo(() => _registry.Remove(A<Guid>._)).MustNotHaveHappened();
    }
}
