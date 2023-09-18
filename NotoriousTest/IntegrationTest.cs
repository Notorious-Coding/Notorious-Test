using Xunit;

namespace NotoriousTest
{
    public abstract class IntegrationTest<T> : IClassFixture<T>, IDisposable where T : Environment
    {
        protected readonly T CurrentEnvironment;

        public IntegrationTest(T environment)
        {
            // Called before each tests
            CurrentEnvironment = environment;
        }

        public void Dispose()
        {
            // Called after each tests
            CurrentEnvironment.Reset();
        }
    }
}
