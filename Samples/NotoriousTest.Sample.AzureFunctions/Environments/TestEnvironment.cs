using System.Reflection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.Sample.AzureFunctions.Infrastructures;

namespace NotoriousTest.Sample.AzureFunctions.Environments;

public class TestEnvironment : EnvironmentBase
{
    public TestEnvironment(EnvironmentSettings settings,
        IWatchDog watchDog,
        IRegistry registry,
        IRuntime runtime,
        ITestLogger logger,
        IServiceProvider serviceProvider) : base(settings,
        watchDog,
        registry,
        runtime,
        logger,
        serviceProvider)
    {
    }

    protected override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override async Task ConfigureEnvironment(InfrastructureListBuilder builder) =>
        // ThenAdd starts a new group, started once the database is running:
        // 1. Database      -> ConnectionStrings:SqlServer
        // 2. Function App  -> consumes ConnectionStrings:SqlServer
        builder.Add<SqlServerInfrastructure>()
            .ThenAdd<FunctionAppInfrastructure>();
}
