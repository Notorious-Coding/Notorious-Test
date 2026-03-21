using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class MultipleInfrastructureTestCasesEnvironment : Environments.Environment
    {
        private readonly Infrastructure _infra1;
        private readonly Infrastructure _infra2;

        public MultipleInfrastructureTestCasesEnvironment(Infrastructure infra1, Infrastructure infra2)
        {
            _infra1 = infra1;
            _infra2 = infra2;
        }

        public override Task ConfigureEnvironment()
        {
            AddInfrastructure(_infra1);
            AddInfrastructure(_infra2);
            return Task.CompletedTask;
        }
    }
}
