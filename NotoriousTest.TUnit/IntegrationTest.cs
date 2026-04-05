namespace NotoriousTest.TUnit
{
    [ClassDataSource(Shared = [SharedType.PerClass])]
    [NotInParallel]
    public abstract class IntegrationTest<T> where T : Environment
    {
        protected readonly T CurrentEnvironment;

        public IntegrationTest(T environment)
        {
            // Called before each tests
            CurrentEnvironment = environment;
        }

        [After(Test)]
        public async ValueTask Reset()
        {
            await CurrentEnvironment.Reset();
        }
    }
}
