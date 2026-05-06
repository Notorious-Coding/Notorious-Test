using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotoriousTest.Core;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;
using NotoriousTest.MSTest.DI;

namespace NotoriousTest.MSTest
{
    [InjectionConfigurator(typeof(MSTestDependencyInjectionConfigurator))]
    [TestClass]
    public abstract class IntegrationTestBase<T> : NotoriousTest.IntegrationTestBase<T> where T : EnvironmentBase
    {
        public static Fixture<T> _fixture;
        public IntegrationTestBase()
        {
            Fixture = _fixture;
        }

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassInitializeAsync(TestContext testContext)
        {
            MSTestDependencyInjectionConfigurator.TestContext = testContext;
            Type? type = GetCurrentTestClass(testContext);
            _fixture = new Fixture<T>(type);
            await _fixture.Environment.Initialize();
        }


        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassCleanupAsync() => await _fixture.Environment.Destroy();

        [TestCleanup]
        public Task TestCleanupAsync() => Environment.Reset();

        private static Type? GetCurrentTestClass(TestContext testContext) =>
            AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(x => x.GetType(testContext.FullyQualifiedTestClassName)).FirstOrDefault(t => t != null);
    }
}
