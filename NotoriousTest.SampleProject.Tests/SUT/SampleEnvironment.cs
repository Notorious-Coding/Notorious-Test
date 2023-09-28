using NotoriousTest.Environments;
using NotoriousTest.Infrastructures;
using NotoriousTest.SampleProject.Tests.SUT.Infrastructures;


namespace NotoriousTest.SampleProject.Tests.SUT
{
    public class SampleEnvironment : AsyncEnvironment
    {
        public override async Task ConfigureEnvironmentAsync(AsyncEnvironmentConfig config)
        {
            config
                .AddInfrastructures(new SampleProjectWebApplicationInfrastructure())
                .AddInfrastructures(new DatabaseInfrastructure());
        }
    }
}
