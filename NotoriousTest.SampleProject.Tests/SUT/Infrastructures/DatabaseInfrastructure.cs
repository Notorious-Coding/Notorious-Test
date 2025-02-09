using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Common.Infrastructures.Sync;
using NotoriousTest.TestContainers.Infrastructures;
using Testcontainers.MsSql;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    public class Test : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer, Configuration>
    {
        protected override MsSqlContainer Container => throw new NotImplementedException();

        public override Task Reset()
        {
            throw new NotImplementedException();
        }
    }
    public class DatabaseInfrastructure : ConfiguredDockerContainerAsyncInfrastructure<MsSqlContainer, Configuration>
    {
        public DatabaseInfrastructure(bool initialize = false): base(initialize)
        {
        }

        public override int? Order => 1;

        protected override MsSqlContainer Container => new MsSqlBuilder().Build();

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
