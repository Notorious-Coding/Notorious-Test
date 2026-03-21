using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class UniqueInfrastructureTestCasesEnvironment : Environments.Environment
    {
        private readonly Infrastructure _infra;

        public UniqueInfrastructureTestCasesEnvironment(Infrastructure infra)
        {
            _infra = infra;
        }

        public override Task ConfigureEnvironment()
        {
            AddInfrastructure(_infra);
            return Task.CompletedTask;
        }
    }
}
