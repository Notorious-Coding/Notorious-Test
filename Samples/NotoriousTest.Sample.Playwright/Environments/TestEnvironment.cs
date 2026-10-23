using System.Reflection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.Sample.Playwright.Infrastructures;
using NotoriousTest.Web.Playwright;

namespace NotoriousTest.Sample.Playwright.Environments;

public class TestEnvironment : EnvironmentBase
{
    protected override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

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

    public override async Task ConfigureEnvironment(InfrastructureListBuilder builder) =>
        // Each ThenAdd starts a new group, started once the previous one is running.
        // Each infrastructure consumes the configuration of the previous ones:
        // 1. Database     -> ConnectionStrings:SqlServer
        // 2. API          -> WebApiUrl
        // 3. React front  -> WebAppUrl
        // 4. Browser      -> opens WebAppUrl
        builder.Add<SqlServerInfrastructure>()
            .ThenAdd<ApiInfrastructure>()
            .ThenAdd<FrontInfrastructure>()
            .ThenAddBrowserFromSettings();
}
