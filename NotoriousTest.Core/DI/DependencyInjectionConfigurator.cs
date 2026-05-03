using Microsoft.Extensions.DependencyInjection;

namespace NotoriousTest.Core.DI;

public class DependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) => services;
}
