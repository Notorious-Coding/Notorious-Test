using NotoriousTest.Core.Environments;
using NotoriousTest.Core.DI;
using NotoriousTest.XUnit.DI;
using Xunit;

namespace NotoriousTest.XUnit
{
    [InjectionConfigurator(typeof(XUnitDependencyInjectionConfigurator))]
    public abstract class IntegrationTest<TEnvironment> : NotoriousTest.IntegrationTestBase<TEnvironment>,
        IClassFixture<XUnitFixture<TEnvironment>>,
        IAsyncDisposable where TEnvironment : EnvironmentBase
    {
        public IntegrationTest(XUnitFixture<TEnvironment> fixture)
        {
            Fixture = fixture;
        }

        public async ValueTask DisposeAsync() => await Environment.Reset();
    }
}
