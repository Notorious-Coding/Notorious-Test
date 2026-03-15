using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public class ConfigurationDictionaryTestCasesInfrastructure2 : Infrastructure
        {
            public ConfigurationDictionaryTestCasesInfrastructure2() : base(false)
            {
            }


            public override Task Destroy()
            {
                return Task.CompletedTask;
            }

            public override Task Initialize()
            {
                AddOutputConfigurationEntry("ConfigurationDictionaryTestCasesInfrastructure2:Key2", "Infra2Key2");

                return Task.CompletedTask;
            }

            public override Task Reset()
            {
                return Task.CompletedTask;
            }
        }
    }
}