using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotoriousTest.Infrastructures;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    public class DatabaseInfrastructure : Infrastructure
    {
        public override int Order => 1;

        public override void Destroy()
        {
        }

        public override void Initialize()
        {
        }

        public override void Reset()
        {
        }
    }
}
