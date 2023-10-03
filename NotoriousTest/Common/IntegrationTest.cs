using Xunit;
using Environment = NotoriousTest.Common.Environments.Environment;

namespace NotoriousTest.Common
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
