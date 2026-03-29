using AwesomeAssertions;

using FakeItEasy;

using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.UnitTests.Stubs;

namespace NotoriousTest.UnitTests.Core;

public class TypedExtensionDispatchTests
{

    private readonly ITestLogger _testLogger;
    private readonly IRegistry _registry;
    private readonly IWatchDog _watchDog;

    public TypedExtensionDispatchTests()
    {
        _testLogger = A.Fake<ITestLogger>();
        _registry = A.Fake<IRegistry>();
        _watchDog = A.Fake<IWatchDog>();
    }
    [Fact]
    public async Task TypedExtension_CalledWithMatchingType_InvokesTypedHooks()
    {
        var infra1 = new InfrastructureStub(_testLogger, _registry);
        InfrastructureStub? actualInfra = null;
        ExtensionStub<InfrastructureStub> extension = new()
        {
            OnBeforeInitializeAction = (infra) =>
            {
                actualInfra = infra;
            }
        };
        infra1.EnsureExtension(extension);

        await infra1.InitializeAsync();

        actualInfra.Should().NotBeNull().And.Be(infra1);

    }

    [Fact]
    public async Task TypedExtension_CalledWithNonMatchingType_DoesNotInvokeTypedHooks()
    {
        var infra1 = new InfrastructureStub(_testLogger, _registry);

        ExtensionStub<DifferentInfrastructureStub> extension = new();
        infra1.EnsureExtension(extension);

        var act = () => infra1.InitializeAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    class DifferentInfrastructureStub : NotoriousTest.Core.Infrastructures.Infrastructure
    {
        public DifferentInfrastructureStub(ITestLogger logger, IRegistry registry) : base(Guid.NewGuid(), logger, registry)
        {
        }

        public override Task Destroy()
        {
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }
}
