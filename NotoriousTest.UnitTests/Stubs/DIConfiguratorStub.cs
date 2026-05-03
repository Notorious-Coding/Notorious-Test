using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Environments;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.Registry;
using NotoriousTest.Core.Runtime;
using NotoriousTest.Core.Watchdog;

namespace NotoriousTest.UnitTests;

public class DIConfiguratorStub : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton(A.Fake<IWatchDog>())
            .AddSingleton(A.Fake<ITestLogger>())
            .AddSingleton(new EnvironmentSettings());
}

public class DIConfiguratorStub2 : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton(A.Fake<IRegistry>())
            .AddSingleton(A.Fake<IRuntime>());
}
