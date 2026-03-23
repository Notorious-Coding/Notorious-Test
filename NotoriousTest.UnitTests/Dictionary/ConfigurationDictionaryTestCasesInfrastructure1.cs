using FakeItEasy;

using NotoriousTest.Infrastructures;
using NotoriousTest.Logger;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class ConfigurationDictionaryTestCasesInfrastructure1 : Infrastructure<string>
    {
        public ConfigurationDictionaryTestCasesInfrastructure1() : base(Guid.NewGuid(), A.Fake<ITestLogger>()) { }
        public override Task Destroy() => Task.CompletedTask;

        public override Task Initialize()
        {
            AddEntry("ConfigurationDictionaryTestCasesInfrastructure1:Key1", "Infra1Key1");
            return Task.CompletedTask;
        }

        public override Task Reset() => Task.CompletedTask;
    }
}
