using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class ConfigurationObjectTestCasesInfrastructure1 : Infrastructure<ConfigurationObjectTestCasesInfrastructureConfiguration1>
    {
        public ConfigurationObjectTestCasesInfrastructure1() : base(Guid.NewGuid()) { }
        public override Task Destroy() => Task.CompletedTask;

        public override Task Initialize()
        {
            AddEntry(nameof(ConfigurationObjectTestCasesInfrastructureConfiguration1), new()
            {
                Key1 = "Infra1Key1"
            });
            return Task.CompletedTask;
        }

        public override Task Reset() => Task.CompletedTask;
    }
}
