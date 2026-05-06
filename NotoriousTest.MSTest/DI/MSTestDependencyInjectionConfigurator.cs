using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Logger;
using NotoriousTest.MSTest.Logger;

namespace NotoriousTest.MSTest.DI;

public class MSTestDependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    internal static TestContext TestContext { get; set; }

    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton(TestContext)
            .AddSingleton<ITestLogger, MSTestTestLogger>();
}
