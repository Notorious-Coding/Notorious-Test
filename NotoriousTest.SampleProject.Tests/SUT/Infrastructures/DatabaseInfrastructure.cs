using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotoriousTest.Common.Infrastructures;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Common.Infrastructures.Sync;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    public class DatabaseInfrastructure : AsyncConfiguredInfrastructure<Configuration>, IConfigurationProducer
    {
        
        public DatabaseInfrastructure(bool initialize = false): base(initialize)
        {
        }
        public override int? Order => 1;

        public override Task Destroy()
        {
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {
            Configuration.DatabaseConfiguration = new DatabaseConfiguration()
            {
                ConnectionString = "Test"
            };
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }
}
