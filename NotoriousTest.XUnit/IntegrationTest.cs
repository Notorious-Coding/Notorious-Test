using System.Threading.Tasks;

using Xunit;

namespace NotoriousTest.XUnit
{
    public abstract class IntegrationTest<T> : IClassFixture<T>, IAsyncLifetime where T : Environment
    {
        protected readonly T CurrentEnvironment;

        public IntegrationTest(T environment)
        {
            // Called before each tests
            CurrentEnvironment = environment;
        }

        public async ValueTask InitializeAsync()
        {
        }

        public async ValueTask DisposeAsync()
        {
            await CurrentEnvironment.Reset();
        }
    }
}
