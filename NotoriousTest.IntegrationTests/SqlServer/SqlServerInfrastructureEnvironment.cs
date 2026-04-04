using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.TestContainers;

using System.Reflection;

using Testcontainers.MsSql;

using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.Environment
{
    public class SqlServerInfrastructureEnvironment : XUnit.Environment
    {
        public SqlServerInfrastructureEnvironment(IMessageSink sink) : base(sink)
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
        public SqlServerInfrastructure(ContextId contextId, ITestLogger logger, IRegistry registry) : base(contextId, logger, registry)
        {
            Container = new MsSqlBuilder().Build();
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
