using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NotoriousTest.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTest.SampleProject.Tests.SUT.Infrastructures
{
    internal class SampleProjectWebApplicationInfrastructure : WebApplicationInfrastructure<Program>
    {
        public SampleProjectWebApplicationInfrastructure(WebApplicationFactory<Program> webApplicationFactory) : base(webApplicationFactory)
        {
        }
    }
}
