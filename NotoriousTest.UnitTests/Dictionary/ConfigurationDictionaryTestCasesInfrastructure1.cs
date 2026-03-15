using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class ConfigurationDictionaryTestCasesInfrastructure1 : Infrastructure
        {
            public ConfigurationDictionaryTestCasesInfrastructure1() : base(false)
            {
            }


            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                AddOutputConfigurationEntry("ConfigurationDictionaryTestCasesInfrastructure1:Key1", "Infra1Key1");

                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }
    }
}