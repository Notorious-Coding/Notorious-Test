using Environment = NotoriousTest.Environments.Environment;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class ConfigurationDictionaryTestCasesEnvironment : Environment
        {
            public ConfigurationDictionaryTestCasesEnvironment()
            {
            }
            public override Task ConfigureEnvironment()
            {
                AddInfrastructure(_configurationDictionaryTestCasesInfrastructure1);
                AddInfrastructure(_configurationDictionaryTestCasesInfrastructure2);
                return Task.CompletedTask;
            }
        }
    }
}