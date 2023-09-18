using NotoriousTest.Infrastructures;
using NotoriousTest.SampleProject.Tests.SUT.Infrastructures;


namespace NotoriousTest.SampleProject.Tests.SUT
{
    public class SampleEnvironment : Environment
    {
        public override void ConfigureEnvironment(EnvironmentConfig config)
        {
            config
                .AddPersistantInfrastructures(new SampleProjectWebApplicationInfrastructure(new SampleProjectApp()))
                .AddPersistantInfrastructures(new DatabaseInfrastructure());
        }
    }
}
