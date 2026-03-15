namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class MultipleInfrastructureTestCasesEnvironment : Environments.Environment
        {

            public MultipleInfrastructureTestCasesEnvironment()
            {

            }

            public override Task ConfigureEnvironment()
            {
                AddInfrastructure(_multipleInfrastructureTestCasesInfrastructure1);
                AddInfrastructure(_multipleInfrastructureTestCasesInfrastructure2);

                return Task.CompletedTask;
            }
        }
    }
}