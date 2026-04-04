using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

using System.Reflection;

using Testcontainers.PostgreSql;

using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.Postgre
{
    public class PostgreInfrastructureEnvironment : XUnit.Environment
    {
        public PostgreInfrastructureEnvironment(IMessageSink sink) : base(sink)
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
        public PostgreServerInfrastructure(ContextId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Container = new PostgreSqlBuilder().Build();
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            AddEntry("ConnectionStrings:Default", Container.GetConnectionString());
        }

        public override async Task Reset()
        {
        }
    }
}
