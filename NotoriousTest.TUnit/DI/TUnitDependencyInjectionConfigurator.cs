using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core.DI;
using NotoriousTest.Core.Logger;
using NotoriousTest.TUnit.Logger;

namespace NotoriousTest.TUnit.DI;

public class TUnitDependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<ITestLogger, TUnitTestLogger>();
}
