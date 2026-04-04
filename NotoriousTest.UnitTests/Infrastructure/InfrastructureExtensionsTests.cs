using AwesomeAssertions;

using FakeItEasy;

using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.UnitTests.Stubs;

namespace NotoriousTest.UnitTests.Infrastructure;

public class InfrastructureExtensionsTests
{

    private readonly ITestLogger _testLogger;
    private readonly IRegistry _registry;

    public InfrastructureExtensionsTests()
    {
        _testLogger = A.Fake<ITestLogger>();
        _registry = A.Fake<IRegistry>();
    }

    [Fact]
    public async Task Extensions_OnBeforeInitialize_CalledBeforeInitialize()
    {
        List<int> order = new();

        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnInitialize = async () =>
            {
                order.Add(2);
            }
        };
        var extension = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension.OnBeforeInitializeAction = (_) =>
        {
            order.Add(1);
        };
        await infrastructure.InitializeAsync();

        order.Should().Equal(1, 2);

    }

    [Fact]
    public async Task Extensions_OnAfterInitialize_CalledAfterInitialize()
    {
        List<int> order = new();

        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnInitialize = async () =>
            {
                order.Add(1);
            }
        };
        var extension = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension.OnAfterInitializeAction = (_) =>
        {
            order.Add(2);
        };

        await infrastructure.InitializeAsync();

        order.Should().Equal(1, 2);
    }

    [Fact]
    public async Task Extensions_OnBeforeReset_CalledBeforeReset()
    {
        List<int> order = new();

        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnReset = async () =>
            {
                order.Add(2);
            }
        };
        var extension = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension.OnBeforeResetAction = (_) =>
        {
            order.Add(1);
        };

        await infrastructure.ResetAsync();

        order.Should().Equal(1, 2);
    }

    [Fact]
    public async Task Extensions_OnAfterReset_CalledAfterReset()
    {
        List<int> order = new();

        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnReset = async () =>
            {
                order.Add(1);
            }
        };
        var extension = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension.OnAfterResetAction = (_) =>
        {
            order.Add(2);
        };

        await infrastructure.ResetAsync();

        order.Should().Equal(1, 2);
    }

    [Fact]
    public async Task Extensions_OnBeforeDestroy_CalledBeforeDestroy()
    {
        List<int> order = new();

        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnDestroy = async () =>
            {
                order.Add(2);
            }
        };

        var extension = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension.OnBeforeDestroyAction = (_) =>
        {
            order.Add(1);
        };

        await infrastructure.DestroyAsync();

        order.Should().Equal(1, 2);
    }

    [Fact]
    public async Task Extensions_OnAfterDestroy_CalledAfterDestroy()
    {
        List<int> order = new();

        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry)
        {
            OnDestroy = async () =>
            {
                order.Add(1);
            }
        };

        var extension = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension.OnAfterDestroyAction = (_) =>
        {
            order.Add(2);
        };

        await infrastructure.DestroyAsync();

        order.Should().Equal(1, 2);
    }
    [Fact]
    public void EnsureExtension_ExistingExtension_ReturnsSameInstance()
    {
        InfrastructureStub infrastructure = new InfrastructureStub(_testLogger, _registry);
        var extension1 = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        var extension2 = infrastructure.EnsureExtension<ExtensionStub<InfrastructureStub>>();
        extension1.Should().BeSameAs(extension2);
    }
}
