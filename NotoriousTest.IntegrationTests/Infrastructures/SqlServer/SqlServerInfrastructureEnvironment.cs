using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

using System.Reflection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using Testcontainers.MsSql;

namespace NotoriousTest.IntegrationTests.Environment
{
    public class SqlServerInfrastructureEnvironment : EnvironmentBase
    {
        public SqlServerInfrastructureEnvironment(EnvironmentSettings settings, IWatchDog watchDog, IRegistry registry, IRuntime runtime, ITestLogger logger, IServiceProvider serviceProvider) : base(settings, watchDog, registry, runtime, logger, serviceProvider)
        {
        }

        public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

        public override async Task ConfigureEnvironment()
        {
            AddInfrastructure<SqlServerInfrastructure>();
        }
    }

    public class SqlServerInfrastructure : DockerContainerInfrastructure<MsSqlContainer, string>
    {
        public SqlServerInfrastructure(EnvironmentId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Container = new MsSqlBuilder().Build();
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            AddEntry("ConnectionStrings:Default", Container.GetConnectionString());
        }
    }
}
