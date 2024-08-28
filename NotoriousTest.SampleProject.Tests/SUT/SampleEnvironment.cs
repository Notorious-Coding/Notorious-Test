using NotoriousTest.SampleProject.Tests.SUT.Infrastructures;
using NotoriousTest.Web.Environments;

namespace NotoriousTest.SampleProject.Tests.SUT
{
    public class SampleEnvironment : AsyncWebEnvironment<Program, Configuration>
    {
        public override Task ConfigureEnvironmentAsync()
        {
            AddInfrastructure(new DatabaseInfrastructure());
            AddWebApplication(new SampleProjectApp());

            return Task.CompletedTask;
        }
    }
}

