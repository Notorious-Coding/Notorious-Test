using FakeItEasy;

using NotoriousTest.Infrastructures;

using Xunit.Sdk;

namespace NotoriousTest.UnitTests;

public partial class AsyncEnvironmentUnitTests
{
    public class UniqueInfrastructureTestCasesEnvironment : Environments.Environment
    {
        private readonly Infrastructure _infra;

        public UniqueInfrastructureTestCasesEnvironment(Infrastructure infra) : base(A.Fake<IMessageSink>())
        {
            _infra = infra;
        }

        public override Task ConfigureEnvironment()
        {
            AddInfrastructure(_infra);
            return Task.CompletedTask;
        }
    }
}
