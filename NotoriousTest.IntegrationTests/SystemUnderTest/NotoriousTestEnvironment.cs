using System.Reflection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;
using NotoriousTest.IntegrationTests.Infrastructure;
using Xunit.Sdk;

namespace NotoriousTest.IntegrationTests.SystemUnderTest;

public class NotoriousTestEnvironment : EnvironmentBase
{
    public NotoriousTestEnvironment(EnvironmentSettings settings, IWatchDog watchDog, IRegistry registry, IRuntime runtime, ITestLogger logger, IServiceProvider serviceProvider) : base(settings, watchDog, registry, runtime, logger, serviceProvider)
    {
    }

    public override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public override Task ConfigureEnvironment() => Task.FromResult(AddInfrastructure<NotoriousTestRegistryInfrastructure>());
}
