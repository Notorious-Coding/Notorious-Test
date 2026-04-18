using AwesomeAssertions;

using FakeItEasy;

using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.UnitTests.Stubs;

namespace NotoriousTest.UnitTests.Infrastructure;

public class InfrastructureLifecycleTests
{
    private readonly ITestLogger _testLogger;
    private readonly IRegistry _registry;

    public InfrastructureLifecycleTests()
    {
        _testLogger = A.Fake<ITestLogger>();
        _registry = A.Fake<IRegistry>();
    }

    [Fact]
    public async Task InitializeAsync_CallsInitialize()
    {
        bool called = false;
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnInitialize = async () => called = true
        };

        await infrastructure.InitializeAsync();

        called.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_RegistersInRegistry()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_DisableRegistry_SkipsRegistration()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: true);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task InitializeAsync_AlreadyRegistered_SkipsRegistration()
    {
        RegisteredInfrastructureStub infrastructure = new RegisteredInfrastructureStub(_testLogger, _registry);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_LogsStartAndCompletionTimes()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _testLogger.Log(A<string>.That.Matches(s => s.Contains($"[{infrastructure.GetType().Name}] Initialization ...")))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _testLogger.Log(A<string>.That.Matches(s => s.Contains($"[{infrastructure.GetType().Name}] Initialization completed in")))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ResetAsync_CallsReset()
    {
        bool called = false;
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnReset = async () => called = true
        };
        await infrastructure.ResetAsync();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task DestroyAsync_CallsDestroy()
    {
        bool called = false;
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnDestroy = async () => called = true
        };
        await infrastructure.DestroyAsync();
        called.Should().BeTrue();
    }

    [Fact]
    public async Task DestroyAsync_RemovesFromRegistry()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        await infrastructure.DestroyAsync();

        A.CallTo(() => _registry.Remove(infrastructure.Id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DestroyAsync_DisableRegistry_SkipsRemove()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: true);
        await infrastructure.DestroyAsync();
        A.CallTo(() => _registry.Remove(infrastructure.Id)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ResetAsync_Should_NotifyResetAtRegistry()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: true);
        await infrastructure.ResetAsync();
        A.CallTo(() => _registry.NotifyReset(infrastructure.Id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DisposeAsync_CallsDestroyAsync()
    {
        var called = false;
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnDestroy = async () => called = true
        };

        await using (infrastructure)
        {

        }

        called.Should().BeTrue();
    }
}
