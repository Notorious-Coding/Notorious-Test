using NotoriousTest.Infrastructures;

namespace NotoriousTest.UnitTests
{

    public partial class AsyncEnvironmentUnitTests
    {
        public abstract class UniqueInfrastructureTestCasesInfrastructure : Infrastructure
        {
            public UniqueInfrastructureTestCasesInfrastructure() : base(false)
            {

            }

        }
    }
}