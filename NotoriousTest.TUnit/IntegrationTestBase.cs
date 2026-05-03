using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;
using NotoriousTest.TUnit.DI;

namespace NotoriousTest.TUnit
{
    [InjectionConfigurator(typeof(TUnitDependencyInjectionConfigurator))]
    [ClassDataSource(Shared = [SharedType.PerClass])]
    [NotInParallel]
    public abstract class IntegrationTestBase<T> : NotoriousTest.IntegrationTestBase<T> where T : EnvironmentBase
    {
        public IntegrationTestBase(TUnitFixture<T> fixture)
        {
            Fixture = fixture;
        }

        [After(Test)]
        public async ValueTask Reset()
        {
            await Environment.Reset();
        }
    }
}
