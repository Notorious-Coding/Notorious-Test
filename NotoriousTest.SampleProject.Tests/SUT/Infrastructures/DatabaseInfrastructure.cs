using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotoriousTest.Common.Infrastructures.Async;
using NotoriousTest.Common.Infrastructures.Sync;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    public class DatabaseInfrastructure : ConfigurableAsyncInfrastructure<DatabaseConfiguration>
    {
        public DatabaseInfrastructure(bool initialize = false) : base(initialize)
        {
        }

        public override int Order => 1;

        public override Task Destroy()
        {
            return Task.CompletedTask;
        }

        public override Task Initialize()
        {   
            Configuration.ConnectionString = "Test";
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            return Task.CompletedTask;
        }
    }

    public class SQLServerDBInfrastructure : AsyncInfrastructure
    {
        public override int Order => throw new NotImplementedException();
        public SQLServerDBInfrastructure(bool initialize = true) : base(initialize)
        {
            
        }

        public override Task Initialize()
        {
            // Here you can create the database
            return Task.CompletedTask;
        }

        public override Task Reset()
        {
            // Here you can empty the database
            return Task.CompletedTask;
        }
        public override Task Destroy()
        {
            // Here you can destroy the database
            return Task.CompletedTask;
        }
    }
}
