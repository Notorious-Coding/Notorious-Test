using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NotoriousTest.MSTest
{
    [TestClass]
    public abstract class IntegrationTest<T> where T : Environment
    {
        private static T? _environment;

        protected T CurrentEnvironment => _environment!;

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassInitializeAsync(TestContext testContext)
        {
            _environment = (T)System.Activator.CreateInstance(typeof(T), testContext)!;
            await _environment.InitializeAsync();
        }

        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassCleanupAsync()
        {
            if (_environment != null)
            {
                await _environment.DestroyAsync();
                _environment = null;
            }
        }

        [TestCleanup]
        public async Task TestCleanupAsync()
        {
            await CurrentEnvironment.Reset();
        }
    }
}
