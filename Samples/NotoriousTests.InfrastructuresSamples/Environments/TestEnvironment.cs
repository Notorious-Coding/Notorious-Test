using Microsoft.AspNetCore.Mvc.Testing;
using NotoriousTest.Common.Environments;
using NotoriousTest.Web.Environments;
using NotoriousTests.InfrastructuresSamples.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotoriousTests.InfrastructuresSamples.Environments
{
    public class TestEnvironment : AsyncWebEnvironment<Program>
    {
        public override async Task ConfigureEnvironmentAsync()
        {
            await AddInfrastructure(new SqlServerInfrastructures { EnvironmentId = EnvironmentId });
            await AddWebApplication(new TestWebApplication());
        }
    }
}
