using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotoriousTest.Core;
using NotoriousTest.Core.Environments;

namespace NotoriousTest.MSTest
{
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
            _fixture = new Fixture<T>(Type.GetType(testContext.FullyQualifiedTestClassName));
            await _fixture.Environment.Initialize();
        }

        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassCleanupAsync() => await _fixture.Environment.Destroy();

        [TestCleanup]
        public Task TestCleanupAsync() => Environment.Reset();
    }
}
