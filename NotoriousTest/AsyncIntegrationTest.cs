using Xunit;
using AsyncEnvironment = NotoriousTest.Environments.AsyncEnvironment;

namespace NotoriousTest
{
    public abstract class AsyncIntegrationTest<T> : IClassFixture<T>, IAsyncLifetime where T : AsyncEnvironment
    {
        protected readonly T CurrentEnvironment;

        public AsyncIntegrationTest(T environment)
        {
            // Called before each tests
            CurrentEnvironment = environment;
        }

        public async Task InitializeAsync()
        {
            // Nothing to do here.
        }

        public async Task DisposeAsync()
        {
            await CurrentEnvironment.ResetAsync();
        }
    }
}
