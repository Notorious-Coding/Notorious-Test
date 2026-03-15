using Environment = NotoriousTest.Environments.Environment;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class ConfigurationObjectTestCasesEnvironment : Environment
        {
            public ConfigurationObjectTestCasesEnvironment()
            {
            }
            public override Task ConfigureEnvironment()
            {
                AddInfrastructure(_configurationObjectTestCasesInfrastructure1);
                AddInfrastructure(_configurationObjectTestCasesInfrastructure2);
                return Task.CompletedTask;
            }
        }
    }
}