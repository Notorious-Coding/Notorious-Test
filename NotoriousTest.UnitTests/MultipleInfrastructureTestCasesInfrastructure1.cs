using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public abstract class MultipleInfrastructureTestCasesInfrastructure1 : Infrastructure
        {

            public MultipleInfrastructureTestCasesInfrastructure1() : base(false)
            {

            }

        }
    }
}