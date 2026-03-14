using Xunit;

namespace NotoriousTest
{
    public abstract class IntegrationTest<T> : IClassFixture<T>, IAsyncLifetime where T : Environments.Environment
    {
        protected readonly T CurrentEnvironment;

        public IntegrationTest(T environment)
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
