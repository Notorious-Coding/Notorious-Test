using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.DI;
using NotoriousTest.NUnit.Logger;

namespace NotoriousTest.XUnit.DI;

public class NUnitDependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<ITestLogger, NUnitTestLogger>();
}
