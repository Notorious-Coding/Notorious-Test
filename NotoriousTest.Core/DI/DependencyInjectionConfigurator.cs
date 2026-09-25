using Microsoft.Extensions.DependencyInjection;

namespace NotoriousTest.Core.DI;

/// <inheritdoc />
public class DependencyInjectionConfigurator : IDependencyInjectionConfigurator
{
    public IServiceCollection ConfigureServices(IServiceCollection services) => services;
}
