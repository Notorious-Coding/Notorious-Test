using Microsoft.Extensions.DependencyInjection;
using NotoriousTest.Core;
using NotoriousTest.Core.Logger;
using NotoriousTest.Core.DI;
using NotoriousTest.XUnit.Logger;
using Xunit;

namespace NotoriousTest.XUnit.DI;

public class XUnitDependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) =>
        services
            .AddSingleton<ITestLogger, XUnitTestLogger>();
}
