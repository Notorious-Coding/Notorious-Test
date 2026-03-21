using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class ConfigurationObjectTestCasesInfrastructureWithSection2 : Infrastructure<ConfigurationObjectTestCasesInfrastructureConfiguration2>
    {
        public override Task Destroy() => Task.CompletedTask;

        public override Task Initialize()
        {
            AddEntry("Toto", new()
            {
                Key2 = "Infra2Key2"
            });
            return Task.CompletedTask;
        }

        public override Task Reset() => Task.CompletedTask;
    }
}
