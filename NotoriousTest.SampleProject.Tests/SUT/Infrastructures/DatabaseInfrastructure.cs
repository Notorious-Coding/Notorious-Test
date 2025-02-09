using NotoriousTest.TestContainers;
using Testcontainers.MsSql;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    public class DatabaseInfrastructure : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer, Configuration>
    {
        public DatabaseInfrastructure(bool initialize = false): base(initialize)
        {
        }

        public override int? Order => 1;

        protected override MsSqlContainer Container { get; init; } = new MsSqlBuilder().Build();

        public override async Task Destroy()
        {
            await base.Destroy();
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            Configuration.DatabaseConfiguration = new DatabaseConfiguration()
            {
                ConnectionString = Container.GetConnectionString(),
            };
        }

        public override async Task Reset()
        {
           
        }
    }
}
