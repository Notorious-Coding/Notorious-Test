using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

using System.Reflection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using Testcontainers.PostgreSql;

using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.Postgre
{
    public class PostgreInfrastructureEnvironment : EnvironmentBase
    {
        public PostgreInfrastructureEnvironment(EnvironmentSettings settings, IWatchDog watchDog, IRegistry registry, IRuntime runtime, ITestLogger logger, IServiceProvider serviceProvider) : base(settings, watchDog, registry, runtime, logger, serviceProvider)
        {
        }

        public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

        public override async Task ConfigureEnvironment()
        {
            AddInfrastructure<PostgreServerInfrastructure>();
        }
    }

    public class PostgreServerInfrastructure : DockerContainerInfrastructure<PostgreSqlContainer, string>
    {
        public PostgreServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry, EnvironmentSettings settings) : base(contextId, logger, registry, settings)
        {
            Container = new PostgreSqlBuilder().Build();
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            AddEntry("ConnectionStrings:Default", Container.GetConnectionString());
        }
    }
}
