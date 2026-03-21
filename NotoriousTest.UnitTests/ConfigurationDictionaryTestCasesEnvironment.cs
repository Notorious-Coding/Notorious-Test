namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class ConfigurationDictionaryTestCasesEnvironment : Environments.Environment
    {
        private readonly ConfigurationDictionaryTestCasesInfrastructure1 _infra1;
        private readonly ConfigurationDictionaryTestCasesInfrastructure2 _infra2;

        public ConfigurationDictionaryTestCasesEnvironment(
            ConfigurationDictionaryTestCasesInfrastructure1 infra1,
            ConfigurationDictionaryTestCasesInfrastructure2 infra2)
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
