using NUnit.Framework;

namespace NotoriousTest.NUnit
{
    [TestFixture]
    public abstract class IntegrationTest<T> where T : Environment
    {
        private static T? _environment;

        protected T CurrentEnvironment => _environment!;

        [OneTimeSetUp]
        public async Task OneTimeSetUpAsync()
        {
            _environment = (T)System.Activator.CreateInstance(typeof(T))!;
            await _environment.InitializeAsync();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDownAsync()
        {
            if (_environment != null)
            {
                await _environment.DestroyAsync();
                _environment = null;
            }
        }

        [TearDown]
        public async Task TearDownAsync()
        {
            await CurrentEnvironment.Reset();
        }
    }
}
