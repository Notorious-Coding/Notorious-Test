using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public abstract class MultipleInfrastructureTestCasesInfrastructure2 : Infrastructure
        {

            public MultipleInfrastructureTestCasesInfrastructure2() : base(false)
            {

            }
        }
    }
}