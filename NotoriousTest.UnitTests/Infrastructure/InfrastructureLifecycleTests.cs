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
    public async Task InitializeAsync_Should_CallInitialize()
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
    public async Task InitializeAsync_Should_RegisterInRegistry()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_Should_SkipRegistration_WhenRegistryDisabled()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: true);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task InitializeAsync_Should_SkipRegistration_WhenAlreadyRegistered()
    {
        RegisteredInfrastructureStub infrastructure = new RegisteredInfrastructureStub(_testLogger, _registry);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _registry.Register(A<InfrastuctureRegistryEntry>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task InitializeAsync_Should_LogStartAndCompletionTimes()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        await infrastructure.InitializeAsync();
        A.CallTo(() => _testLogger.Log(A<string>.That.Matches(s => s.Contains($"[{infrastructure.GetType().Name}] Initialization ...")))).MustHaveHappenedOnceExactly();
        A.CallTo(() => _testLogger.Log(A<string>.That.Matches(s => s.Contains($"[{infrastructure.GetType().Name}] Initialization completed in")))).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ResetAsync_Should_CallReset()
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
    public async Task DestroyAsync_Should_CallDestroy()
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
    public async Task DestroyAsync_Should_RemoveFromRegistry()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        await infrastructure.DestroyAsync();

        A.CallTo(() => _registry.Remove(infrastructure.Id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task DestroyAsync_Should_SkipRemove_WhenRegistryDisabled()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: true);
        await infrastructure.DestroyAsync();
        A.CallTo(() => _registry.Remove(infrastructure.Id)).MustNotHaveHappened();
    }

    [Fact]
    public async Task ResetAsync_Should_NotifyResetAtRegistry()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: false);
        await infrastructure.ResetAsync();
        A.CallTo(() => _registry.NotifyReset(infrastructure.Id)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task ResetAsync_Should_NotNotifyResetAtRegistry_WhenRegistryDisabled()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry, disableRegistry: true);
        await infrastructure.ResetAsync();
        A.CallTo(() => _registry.NotifyReset(infrastructure.Id)).MustNotHaveHappened();
    }

    [Fact]
    public async Task DisposeAsync_Should_CallDestroyAsync()
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
