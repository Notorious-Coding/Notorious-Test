using FakeItEasy;

using Xunit.Sdk;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class ConfigurationObjectTestCasesEnvironment : Environments.EnvironmentBase
    {
        private readonly ConfigurationObjectTestCasesInfrastructure1 _infra1;
        private readonly ConfigurationObjectTestCasesInfrastructureWithSection2 _infra2;

        public ConfigurationObjectTestCasesEnvironment(
            ConfigurationObjectTestCasesInfrastructure1 infra1,
            ConfigurationObjectTestCasesInfrastructureWithSection2 infra2) : base(A.Fake<IMessageSink>())
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
