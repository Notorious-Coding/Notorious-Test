namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class UniqueInfrastructureTestCasesEnvironment : Environments.Environment
        {

            public UniqueInfrastructureTestCasesEnvironment()
            {

            }

            public override Task ConfigureEnvironment()
            {
                AddInfrastructure(_uniqueInfrastructureTestCasesInfrastructure);

                return Task.CompletedTask;
            }
        }
    }
}