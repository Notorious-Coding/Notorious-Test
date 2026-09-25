using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;

namespace NotoriousTest.UnitTests.Stubs;

public class EnvironmentStub : EnvironmentBase
{
    public EnvironmentStub(EnvironmentSettings settings, IWatchDog watchDog, IRegistry registry, IRuntime runtime,
        ITestLogger logger, IServiceProvider serviceProvider) : base(settings, watchDog, registry, runtime, logger,
        serviceProvider)
    {
    }

    protected override Assembly CurrentAssembly => Assembly.GetExecutingAssembly();

    public Func<Task>? OnConfigureEnvironment { get; set; }
    public Action<IServiceCollection>? OnConfigureInfrastructureServices { get; set; }

    public IServiceProvider PublicServiceProvider => ServiceProvider;
    public override Task ConfigureEnvironment() => OnConfigureEnvironment?.Invoke() ?? Task.CompletedTask;
}
