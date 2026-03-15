using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class ConfigurationObjectTestCasesInfrastructureWithSection2 : Infrastructure<ConfigurationObjectTestCasesInfrastructureConfiguraton2>
        {


            public ConfigurationObjectTestCasesInfrastructureWithSection2() : base(false)
            {
            }


            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                AddOutputConfigurationEntry("Toto", new()
                {
                    Key2 = "Infra2Key2"
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