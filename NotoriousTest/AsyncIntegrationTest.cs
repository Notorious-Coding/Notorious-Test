using NotoriousTest.Environments;

using Xunit;

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

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await CurrentEnvironment.Reset();
        }
    }
}
