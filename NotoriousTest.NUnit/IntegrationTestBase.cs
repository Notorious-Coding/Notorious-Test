using NotoriousTest.Core;
using NotoriousTest.Core.Environments;
using NUnit.Framework;
using NotoriousTest.Core.DI;
using NotoriousTest.XUnit.DI;

namespace NotoriousTest.NUnit
{
    [InjectionConfigurator(typeof(NUnitDependencyInjectionConfigurator))]
    [TestFixture]
    public abstract class IntegrationTestBase<T> : NotoriousTest.IntegrationTestBase<T> where T : EnvironmentBase
    {
        public IntegrationTestBase()
        {
            Fixture = new Fixture<T>(GetType());
        }

        [OneTimeSetUp]
        public Task OneTimeSetUpAsync() => Environment.Initialize();

        [OneTimeTearDown]
        public Task OneTimeTearDownAsync() => Environment.Destroy();

        [TearDown]
        public Task TearDownAsync() => Environment.Reset();
    }
}
