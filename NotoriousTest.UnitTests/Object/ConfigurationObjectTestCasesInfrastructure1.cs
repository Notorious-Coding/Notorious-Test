using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class ConfigurationObjectTestCasesInfrastructure1 : Infrastructure<ConfigurationObjectTestCasesInfrastructureConfiguraton1>
        {
            public ConfigurationObjectTestCasesInfrastructure1() : base(false)
            {
            }


            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                AddOutputConfigurationEntry(new()
                {
                    Key1 = "Infra1Key1"
                });
                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }
    }
}