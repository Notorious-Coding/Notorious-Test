using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class ConfigurationDictionaryTestCasesInfrastructure2 : Infrastructure<string>
    {
        public ConfigurationDictionaryTestCasesInfrastructure2() : base(Guid.NewGuid()) { }
        public override Task Destroy() => Task.CompletedTask;

        public override Task Initialize()
        {
            AddEntry("ConfigurationDictionaryTestCasesInfrastructure2:Key2", "Infra2Key2");
            return Task.CompletedTask;
        }

        public override Task Reset() => Task.CompletedTask;
    }
}
