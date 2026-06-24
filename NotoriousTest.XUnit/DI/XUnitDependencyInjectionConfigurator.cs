using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Logger;
using NotoriousTest.XUnit.Logger;

namespace NotoriousTest.XUnit.DI;

public class XUnitDependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<ITestLogger, XUnitTestLogger>();
}
