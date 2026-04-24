using Xunit;

namespace NotoriousTest
{
    /// <summary>
    /// Base class for integration tests that use a shared xUnit class fixture environment.
    /// </summary>
    /// <typeparam name="T">The environment type, must inherit from <see cref="Environments.Environment"/>.</typeparam>
    public abstract class IntegrationTest<T> : IClassFixture<T>, IAsyncLifetime where T : Environments.Environment
    {
        /// <summary>Gets the current test environment shared across tests in the class.</summary>
        protected readonly T CurrentEnvironment;

        /// <summary>Initializes a new instance with the shared environment provided by xUnit.</summary>
        /// <param name="environment">The shared test environment instance.</param>
        public IntegrationTest(T environment)
        {
            // Called before each tests
            CurrentEnvironment = environment;
        }

        /// <summary>Called by xUnit before each test method; override to add per-test setup.</summary>
        public async ValueTask InitializeAsync()
        {
        }

        /// <summary>Resets the environment after each test method.</summary>
        public async ValueTask DisposeAsync()
        {
            await CurrentEnvironment.Reset();
        }
    }
}
