using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotoriousTest.Infrastructures;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    public class DatabaseInfrastructure : AsyncInfrastructure
    {
        public DatabaseInfrastructure(bool initialize = false) : base(initialize)
        {
        }

        public override int Order => 1;

        public override async Task Destroy()
        {
        }

        public override async Task Initialize()
        {
        }

        public override async Task Reset()
        {
        }
    }
}
